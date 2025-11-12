# MAUI Interop Demo

This repository contains a demo project which recorded how I setup a minimal demo to achieve interoperability between .NET MAUI and Swift for iOS development.

When you click on the button in the app, it will call a Swift function log a hello world message.

![MAUI App Screenshot](/Doc/HelloWorld-ios.png)

![Hello World Printed](/Doc/HelloWorld-ios.png)

## Project Structure
- Dotnet: .NET MAUI project
    - MainPage contains a button to call Swift function via C-ABI.
    - Uses `[DllImport]` to import the Swift function from static library.
    - Use `NativeReference` in the project file to link the static library.
    ```xml
    <ItemGroup>
		<NativeReference Include="Platforms/iOS/Frameworks/libHelloWorldBridge.a">
			<Kind>Static</Kind>
			<ForceLoad>true</ForceLoad>
		</NativeReference>
	</ItemGroup>
    ```
- Native/iOS: An xcode project contains a static library target with a simplest Swift function. 
    - Function exported to C-ABI with `@_cdecl` to be called from .NET MAUI project.
    - Compiled to a static library `.a` file.

## Instructions 
1. Open the `Native/iOS/HelloWorldBridge.xcodeproj` in Xcode.
2. Build the project for `Any iOS Simulator Device (arm64)` target, simply (Product > Build) or (Command + B).
3. Find the compiled static library file `libHelloWorldBridge.a`
    - (Product > Show Build Folder in Finder)
    - (Build folder path) > Products > Debug-iphonesimulator > `libHelloWorldBridge.a`
    - Copy the `libHelloWorldBridge.a` file to `Dotnet/Platforms/iOS/Frameworks/` folder.
4. Build and run the .NET MAUI project on iOS simulator.
   ```bash
   dotnet build -t:Run -f net10.0-ios -r iossimulator-arm64
   ```

## Save time from manual copying the static library file
Since there will be different build configurations, xcode build folders for simulator and device. To avoid manually copying the static library file from different place every time, you can add a MSBuild target in the .NET MAUI project file to automate the copy process based on the `-r` (RuntimeIdentifier) and `-c` (Configuration) flags passed to `dotnet build`.

Add the below into `Dotnet/maui-interop-demo.csproj`:
```xml
	<!-- View messages by -v:detailed flag with dotnet build -->
	<Target Name="CopyXcodeStaticLib" BeforeTargets="PrepareForBuild">
		<PropertyGroup>
			<!-- Fallback: if dotnet build didn't pass -c, use Debug -->
    		<EffectiveConfiguration Condition="'$(Configuration)' == ''">Debug</EffectiveConfiguration>
    		<EffectiveConfiguration Condition="'$(Configuration)' != ''">$(Configuration)</EffectiveConfiguration>

			<!-- Flags derived from -r value -->
			<IsSimulator>$([System.String]::Copy('$(RuntimeIdentifier)').StartsWith('iossimulator-'))</IsSimulator>
			<IsDevice>$([System.String]::Copy('$(RuntimeIdentifier)').StartsWith('ios-'))</IsDevice>

			<!-- Where to drop the lib so NativeReference can find it -->
			<NativeDestDir>$(MSBuildProjectDirectory)/Platforms/iOS/Frameworks</NativeDestDir>
			<NativeLibName>libHelloWorldBridge.a</NativeLibName>

			<!-- Replace with your Xcode project build folder path -->
			<XcodeBuildDir>$(HOME)/Library/Developer/Xcode/DerivedData/HelloWorldBridge-gqrzvpbxfvvqoyfhmygoqruiwpao/Build/Products</XcodeBuildDir>
			<XcodeProductsDirSim>$(XcodeBuildDir)/$(EffectiveConfiguration)-iphonesimulator</XcodeProductsDirSim>
			<XcodeProductsDirDev>$(XcodeBuildDir)/$(EffectiveConfiguration)-iphoneos</XcodeProductsDirDev>

			<!-- Prefer simulator if building for simulator, else device -->
			<StaticLibSrcSim>$(XcodeProductsDirSim)/$(NativeLibName)</StaticLibSrcSim>
			<StaticLibSrcDev>$(XcodeProductsDirDev)/$(NativeLibName)</StaticLibSrcDev>
		</PropertyGroup>

		<Message Importance="High" Text="Using configuration: $(EffectiveConfiguration)" />
		<RemoveDir Directories="$(NativeDestDir)" />
		<MakeDir Directories="$(NativeDestDir)" />

		<Copy Condition="'$(IsSimulator)' == 'True' AND Exists('$(StaticLibSrcSim)')"
				SourceFiles="$(StaticLibSrcSim)"
				DestinationFiles="$(NativeDestDir)/$(NativeLibName)"
				SkipUnchangedFiles="true" />
		<Message Condition="'$(IsSimulator)' == 'True'"
				Importance="High"
				Text="RID=$(RuntimeIdentifier) → copied simulator slice: $(StaticLibSrcSim)" />

		<!-- If -r ios-*, copy device slice -->
		<Copy Condition="'$(IsDevice)' == 'True' AND Exists('$(StaticLibSrcDev)')"
				SourceFiles="$(StaticLibSrcDev)"
				DestinationFiles="$(NativeDestDir)/$(NativeLibName)"
				SkipUnchangedFiles="true" />
		<Message Condition="'$(IsDevice)' == 'True'"
				Importance="High"
				Text="RID=$(RuntimeIdentifier) → copied device slice: $(StaticLibSrcDev)" />

		<!-- Helpful warning when nothing matched -->
		<Warning Condition="'$(IsSimulator)' != 'True' AND '$(IsDevice)' != 'True'"
				Text="RuntimeIdentifier '$(RuntimeIdentifier)' not recognised (expected iossimulator-* or ios-*). No copy performed." />		
		
		<ItemGroup>
			<!-- Must match the actual file you copied -->
			<NativeReference Include="$(NativeDestDir)/$(NativeLibName)">
				<Kind>Static</Kind>
				<ForceLoad>true</ForceLoad>
			</NativeReference>
		</ItemGroup>
	</Target>
```