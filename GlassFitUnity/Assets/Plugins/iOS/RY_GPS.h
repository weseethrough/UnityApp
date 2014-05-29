//
//  RY_GPS.h
//  Unity-iPhone
//
//  Created by Andrew Haith on 22/05/2014.
//
//

#import <Foundation/Foundation.h>
#import <CoreLocation/CoreLocation.h>

extern void UnitySendMessage(const char*, const char*, const char*);

@interface RY_GPS : NSObject <CLLocationManagerDelegate>
{
    CLLocationManager* locationManager;
    CLLocation* lastPosition;
    CLHeading* lastHeading;
}
+(RY_GPS*)sharedInstance;
-(void)startGPS;
-(void)stopGPS;

//returns dict with double:lat, double:long, double:heading
-(NSDictionary*)getLatestLocationData;

@end
