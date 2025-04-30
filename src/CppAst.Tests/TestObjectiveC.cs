// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace CppAst.Tests;

public class TestObjectiveC : InlineTestBase
{
    [Test]
    public void TestSimple()
    {
        ParseAssert("""
                    // SimpleHeader.h
                    //#import <Foundation/Foundation.h>
                    //@import Foundation;
                    
                    // Fake Foundation enum declaration for options
                    typedef enum __attribute__((flag_enum)) MyOptions : int MyOptions;
                    enum MyOptions : int {
                    	MyOptions1 = 1,
                    	MyOptions2 = 2,
                    	MyOptions3 = 4,
                    };
                    
                    @protocol MyProtocol
                    + (void)doSomething;
                    @end
                    
                    @protocol MyProtocol2
                    + (void)doSomething2;
                    @end
                    
                    @interface TesterBase <MyProtocol, MyProtocol2>
                    @end
                    
                    @interface SimpleThing : TesterBase
                    
                    /// A read-write string property
                    @property (readonly) const char *name;
                    
                    @property int id;
                    
                    /// A class factory method
                    + (instancetype)thingWithName:(const char *)name;
                    
                    /// An instance method that returns a description
                    - (const char *)describe;
                    
                    - (void)runWithCompletion:(void (^)(int result))completion;
                    
                    @end
                    
                    @interface SimpleThing (MyCategory)
                    @property int categoryId;
                    @end
                    
                    @interface SimpleTemplate<TArg> : SimpleThing
                    - (TArg)get_at:(int)index; 
                    @end
                    
                    @interface SimpleTemplate2<TArg1> : SimpleTemplate<TArg1>
                    @end
                    
                    """,
            compilation =>
            {
                Assert.False(compilation.HasErrors);
                
                
            }, new()
            {
                ParseAsCpp = false,
                TargetCpu = CppTargetCpu.ARM64,
                TargetVendor = "apple",
                TargetSystem = "darwin",
                ParseMacros = false,
                ParseSystemIncludes = true,
                AdditionalArguments =
                {
                    "-x", "objective-c",
                    "-std=gnu11",
                    //"-ObjC",
                    //"-fblocks",
                    //"-fno-modules",
                    //"-fmodules",
                    //"-fimplicit-module-maps",
                    //"-fno-implicit-modules",
                    "-isysroot", "/Applications/Xcode.app/Contents/Developer/Platforms/MacOSX.platform/Developer/SDKs/MacOSX.sdk",
                    "-F", "/Applications/Xcode.app/Contents/Developer/Platforms/MacOSX.platform/Developer/SDKs/MacOSX.sdk/System/Library/Frameworks",
                    "-isystem", "/Applications/Xcode.app/Contents/Developer/Platforms/MacOSX.platform/Developer/SDKs/MacOSX.sdk/usr/include",
                    "-isystem", "/Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/lib/clang/16/include",
                }
            }
        );
    }
}