//
//  Platform.m
//  Unity-iPhone
//
//  Created by Andrew Haith on 24/04/2014.
//
//

#import "Platform.h"
#import <sys/utsname.h>


@implementation Platform

extern "C" {
    
    void _Poll() {
//        NSLog(@"Platform Poll native call");
    }
    
    void _Update() {
//        NSLog(@"Platform Update native call");
    }
    
    void _IsPluggedIn() {
//        NSLog(@"Platform IsPluggedIn native call");
    }
    
    const char* _getDeviceInfo() {
        struct utsname systemInfo;
        uname(&systemInfo);
        
        char* result = (char*)malloc(strlen(systemInfo.machine)+1);
        strcpy(result, systemInfo.machine);
        
        return result;
    }
    
    void _setBadgeNumber(int number) {
        [[UIApplication sharedApplication] setApplicationIconBadgeNumber:number];
    }
    
}




@end
