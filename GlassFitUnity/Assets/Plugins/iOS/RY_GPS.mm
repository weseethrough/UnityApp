//
//  RY_GPS.m
//  Unity-iPhone
//
//  Created by Andrew Haith on 22/05/2014.
//
//

#import "RY_GPS.h"


#define MAX_LOCATION_AGE 5.0


static RY_GPS* shared_instance = nil;

@implementation RY_GPS

extern "C" {
    //returns a JSON object containing double:latitude, double:longitude, double:heading
    char* _getLatestPosition()
    {
        NSDictionary* locationDict = [[RY_GPS sharedInstance] getLatestLocationData];
        
        NSError* error;
        NSData* jsonData = [NSJSONSerialization dataWithJSONObject:locationDict options:0 error:&error];
        NSString* jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
        
        NSLog(@"ios Latest position: %@", jsonString);
        
        const char* jsonChar = [jsonString UTF8String];
        
        char* res = (char*)malloc((strlen(jsonChar)+1));
        strcpy(res, jsonChar);
        
        return res;
        //        NSString* jsonString = locationDict
    }
    
    void _startGPS()
    {
        [[RY_GPS sharedInstance] startGPS];
    }
    
    void _stopGPS()
    {
        [[RY_GPS sharedInstance] stopGPS];
    }
}

+(RY_GPS *)sharedInstance
{
    if(shared_instance == nil)
    {
        shared_instance = [[RY_GPS alloc] init];
    }
    return shared_instance;
}

- (instancetype)init
{
    self = [super init];
    if (self) {
        locationManager = [[CLLocationManager alloc] init];
        locationManager.delegate = self;
        
        locationManager.desiredAccuracy = kCLLocationAccuracyBest;
        // Highest accuracy, only recommended when connected to charger
        //locationManager.desiredAccuracy = kCLLocationAccuracyBestForNavigation;
        
        //receive all updates
        locationManager.distanceFilter = 0;
    }
    return self;
}

- (void)startGPS
{
    [locationManager startUpdatingLocation];
    [locationManager startUpdatingHeading];
}

-(void)stopGPS
{
    [locationManager stopUpdatingHeading];
    [locationManager stopUpdatingLocation];
}

- (void)dealloc
{
    if(locationManager != nil)
    {
        [locationManager release];
    }
    if(lastPosition != nil)
    {
        [lastPosition release];
    }
    if(lastHeading != nil)
    {
        [lastHeading release];
    }
    [super dealloc];
}


-(void)locationManager:(CLLocationManager *)manager didUpdateLocations:(NSArray *)locations
{
    
    CLLocation* location = [locations lastObject];
    NSDate* eventDate = location.timestamp;
    NSTimeInterval age = [eventDate timeIntervalSinceNow];
    if(abs(age) < MAX_LOCATION_AGE)
    {
        if(lastPosition != nil)
        {
            [lastPosition release];
        }
        lastPosition = [location retain];
    }
}

-(void)locationManager:(CLLocationManager *)manager didUpdateHeading:(CLHeading *)newHeading
{
    if(lastHeading != nil)
    {
        [lastHeading release];
    }
    lastHeading = [newHeading retain];
}


-(NSDictionary *)getLatestLocationData
{
    float latitude = 0;
    float longitude = 0;
    float heading = 0;
    float course = 0;
    float epe = 0;
    float speed = 0;
    double ts = 0;
    if(lastPosition != nil)
    {
        latitude = (float)lastPosition.coordinate.latitude;
        longitude = (float)lastPosition.coordinate.longitude;
        course = (float)lastPosition.course;
        epe = (float)lastPosition.horizontalAccuracy;
        speed = (float)lastPosition.speed;
        ts = lastPosition.timestamp.timeIntervalSince1970;
    }
    if(lastHeading != nil)
    {
        heading = (float)lastHeading.trueHeading;
    }
    
    //fill out dictionary
    NSDictionary* locationDict = [[[NSMutableDictionary alloc] initWithCapacity:3] autorelease];
    [locationDict setValue:[NSNumber numberWithFloat:latitude] forKey:@"latitude"];
    [locationDict setValue:[NSNumber numberWithFloat:longitude] forKey:@"longitude"];
    [locationDict setValue:[NSNumber numberWithFloat:heading] forKey:@"heading"];
    [locationDict setValue:[NSNumber numberWithFloat:course] forKey:@"course"];
    [locationDict setValue:[NSNumber numberWithFloat:epe] forKey:@"epe"];
    [locationDict setValue:[NSNumber numberWithFloat:speed] forKey:@"speed"];
    [locationDict setValue:[NSNumber numberWithDouble:ts] forKey:@"ts"];
    return locationDict;
}

-(void)locationManager:(CLLocationManager *)manager didFailWithError:(NSError *)error
{
//    const char* errorMessage = [[[error.userInfo objectForKey:NSLocalizedDescriptionKey] localizedDescription] UTF8String];
//    UnitySendMessage("Platform", "OnLocationError", errorMessage);
}





@end
