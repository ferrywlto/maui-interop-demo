//
//  HelloWorldBridge.swift
//  HelloWorldBridge
//
//  Created by Ferry To on 12/11/2025.
//
import Foundation

@_cdecl("HelloWorldBridge_SayHello")
public func HelloWorldBridge_SayHello() {
    NSLog("👋 Hello World from Swift (Static Library)!")
}
