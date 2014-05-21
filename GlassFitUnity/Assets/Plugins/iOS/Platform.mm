//
//  Platform.m
//  Unity-iPhone
//
//  Created by Andrew Haith on 24/04/2014.
//
//

#import "Platform.h"
#import <sys/utsname.h>
#import <UXCam/UXCam.h>



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
    
    void _StartUXCam() {
        [UXCam startApplicationWithKey:@"13c4e2543331265"];
    }
    
    void _StopUXCamAndUploadData() {
        [UXCam stopApplicationAndUploadData];
    }
    
    void _UXCamTagScreenName(char* screenName) {
        NSString* sName = [NSString stringWithCString:screenName encoding:NSUTF8StringEncoding];
        [UXCam tagScreenName:sName];
        NSLog(@"Tagged screen name for UXCam as: %@", sName);
    }
    
    void _UXCamTagUserName(char* userName, char* additionalData) {
        NSString* sUser = [NSString stringWithCString:userName encoding:NSUTF8StringEncoding];
        NSString* sData = [NSString stringWithCString:additionalData encoding:NSUTF8StringEncoding];
        [UXCam tagUsersName:sUser additionalData:sData];
        NSLog(@"Tagged user for UXCam as %@, with data %@", sUser, sData);
    }
}




@end
