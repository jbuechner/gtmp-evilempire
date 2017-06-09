# Evil Empire RP Server Modification for GT-MP
This project hosts the Evil Empire RP Server Modification written for the GT-MP GTA V server emulation.

# Overview
The modification is self-contained and the distribution contains everything required to run a modified GT MP server, including the server itself. The modified server is started using the `gtmp.evilempire.server.launcher.exe`. The launcher checks the integrity of the distribution before running the server itself. Starting the GT MP server without using the launcher can lead to unwanted behaviour or even errors.

However to be able to run the modification some requirements must be met.

## `settings.xml` required by GT MP
The launcher merges the `settings.template.xml` and `settings.user.xml` into a single `settings.xml` used by the GT MP server during startup. While the template is part of the distribution the user file is not and needs to be created.

The template file should not be changed. If changes are necessary they must be done in the `settings.user.xml`. Template and User file share the same schema. User definitions will always override template definitions.

An exemplary user settings file can look like this:
```xml
<?xml version="1.0"?>
<config xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <servername>[GER|DEV] Evil Empire Hardcore Roleplay - Development Instance</servername>
  <maxplayers>10</maxplayers>
  <acl_enabled>false</acl_enabled>
  <loglevel>1</loglevel>
  <log>true</log>
  <allow_client_devtools>true</allow_client_devtools>
  <conntimeout>false</conntimeout>
</config>
```

## Database Templates
The modification will create an embedded database on its own using the `db` subdirectory of the distribution. However the launcher requires a directory of database templates, named `dbt`. Templates are utilized by the launcher to populate data into the database. Templates are executed every time the launcher runs.

Each `*.xml` file inside the `dbt` directory is considered to be a template. The launcher will read each template file and tries to insert new entities or update existing ones inside the database.


At the moment the `dbt` directory is not part of the distribution.

The root element of each database template file is the `elements` element. It must have the `type` attribute containing a fully qualified type name to point to the correct entity type. Beneath the root element there can be multiple entities of the type defined. Entity elements must use the (case-sensitive) type name of the entity.

In addition the root element might specify a custom (post) processor. The processor is used to alter the entities after they have been read from the template file and before they are inserted/updated inside the database. The custom processor must be specified using a fully qualified type named and must implement the `IEntityProcessor` interface.

### User template file
You might want a user database template file to create users during startup to be able to login to the server. An example of a user template file is:

```xml
<?xml version="1.0" encoding="utf-8"?>
<elements type="gtmp.evilempire.entities.User, gtmp.evilempire.shared" customProcessor="gtmp.evilempire.entities.processors.UserPasswordHashProcessor, gtmp.evilempire.shared">
    <User>
        <Login>user</Login>
        <Password>::fe4873ee8e66fc12ff940d6a21e1ff73962874c56bbed7f41c95247a086ebb362797e60cd413e6f5ceada814d30a96349038686e365a09ecc030d9ad188286c3</Password>
    </User>
    <User>
        <Login>admin</Login>
        <Password>::3fed5fe0a6b8bf9e8cf91e59647c9b2e8ff8394835f4d8f6a3db2b0a5c8875c0c5936e5b255b9158f03fe3561cd0585b13cfd617b9f23e8c9f464ec128b05bb3</Password>
    </User>
</elements>
```

Please notice that the Passwords are pre-hashed and not in plain text. The file must contain the same password message as it comes from the client implementation. The client utilizes SHA512 to pre-hash the plain password before sending it to the server. Therefore passwords inside this file must be in the same format. To generate passwords the npm script `cshash` can be used. The script is part of the repository but you need npm installed to be able to use it.

## Map files
Map files are used to describe either dynamic or static content of the modification. Map files are not limited to mapping related content and include meta data too. Therefore the term map file is technically not the most accurate to describe the usage and content of these files.

Map files are using the same structure produced by the GTA V map editor and extending the format.

Map files are xml files in a *maps* sub directory next to the server executable.

Each map file can contain a portion of the whole map and meta data required to launch and run the server. Therefore information can be separated into more logical or editable units.

### Structure
Map files are plain xml files using the `Map` element as root. Element and attribute names, unless specified otherwise, are case-sensitive. Map files should be encoded in `UTF-8`.

Excessive information inside the XML file is discarded regardless of their meaning. Only xml elements and attributes describes within the following sections are interpreted by the map loader.

A raw map template for a map file looks like this:
```xml
<?xml version="1.0" encoding="utf-8"?>
<Map>
</Map>
```

### Common map file elements
Common elements used inside the map files are `Vector3f` elements. They describe a coordinate in a 3D environment.

Example of a `Vector3f` element:
```
<X>0.0</X>
<Y>-10.2</X>
<Z>209.8</Z>
```

`Vector3f` elements are never used as a standalone description but are used semantically for other descriptions like position, rotation, scale, etc.

Element     | Data Type     | Required  | Default
------------|---------------|-----------|----------
X           | Float         | No        | 0.0
Y           | Float         | No        | 0.0
Z           | Float         | No        | 0.0

### Use map files to define map related content
A common use case is to define certain map related aspects inside the map files. Possible objects are `Props`, `Vehicles` and `Peds`.

#### Props
Props are entities synchronized between the GTMP server and client. Props are defined as `MapObject` elements of the `Type` `Prop`.

A definition inside a map file looks like:
```xml
<?xml version="1.0" encoding="utf-8"?>
<Map>
  <Objects>
    <MapObject>
      <Type>Prop</Type>
      <Hash>16805345</Hash>
      <TemplateName>A templated Prop</TemplateName>
      <Position>
        <X>435.9383</X>
        <Y>-994.26886</Y>
        <Z>31.9168968</Z>
      </Position>
      <Rotation>
        <X>0</X>
        <Y>0</Y>
        <Z>84.99959</Z>
      </Rotation>
      <IsPositionFrozen>true</IsPositionFrozen>
      <IsCollisionless>true</IsCollisionless>
    </MapObject>
  </Objects>
</Map>
```

Element                 | Data Type     | Required  | Default   | Description
------------------------|---------------|-----------|-----------|-------------------------------------------------------
Type                    | String        | Yes       | *n/a*     | Must be `Prop` to be a prop entity.
Hash                    | Int32         | Yes       | *n/a*     | A valid hash value. The map loader will warn about invalid values.
TemplateName            | String        | No        | *n/a*     | Optional name of the template. Templates are not spawned but are used by other functionality to spawn a entity describes by the template.
Position                | Vector3f      | No        | 0,0,0     | Position of the entity in game world coordinates.
Rotation                | Vector3f      | No        | 0,0,0     | Rotation of the entity in game world coordinates.
IsPositionFrozen        | Bool          | No        | false     | Whether the entity is frozen or not. Frozen entities should be unable to move on their own or by force acting upon them.
IsCollisionless         | Bool          | No        | false     | Whether the entity is collisionless and other entities pass through the entity.

The `color` element is used to define a single color value inside RGB color space. A exemplary definition looks like:

```xml
<Alpha>0</Alpha>
<Red>255</Red>
<Green>55</Green>
<Blue>34</Blue>
```

Element | Data Type     | Required  | Default   | Description
--------|---------------|-----------|-----------|--------------------------------
Alpha   | Byte          | No        | 0         | Describes the Alpha component of the color. Between 0 (full opacity) and 255 (no opacity). Please not that transparency can not be used for every color definition.
Red     | Byte          | No        | 0         | Red component.
Green   | Byte          | No        | 0         | Green component.
Blue    | Byte          | no        | 0         | Blue component.

#### Vehicles
Vehicles are entities synchronized between the GTMP server and client. Vehicles are defined as `MapObject` elements of `Type` `Vehicle`.

A definition inside a map file looks like:
```xml
<?xml version="1.0" encoding="utf-8"?>
<Map xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
    <Objects>
        <MapObject>
            <Type>Vehicle</Type>
            <Hash>237764926</Hash>
            <TemplateName>Templated Vehicle</TemplateName>
            <Position>
                <X>-1004</X>
                <Y>-2708</Y>
                <Z>14</Z>
            </Position>
            <Rotation>
                <Y>-32</Y>
            </Rotation>
            <Dynamic>true</Dynamic>
            <Quaternion>
                <X>-0.0183549933</X>
                <Y>0.009907198</Y>
                <Z>0.0341101177</Z>
                <W>0.9992004</W>
            </Quaternion>
            <SirensActive>false</SirensActive>
            <PrimaryColor>111</PrimaryColor>
            <SecondaryColor>0</SecondaryColor>

            <!--Custom-->
            <IsInvincible>true</IsInvincible>
            <IsEngineRunning>false</IsEngineRunning>
            <IsLocked>true</IsLocked>
            <IsCollisionless>false</IsCollisionless>
            <HasBulletproofTyres>false</HasBulletproofTyres>
            <IsPositionFrozen>false</IsPositionFrozen>
            <NumberPlate>A-1123</NumberPlate>
            <NumberPlateStyle>2</NumberPlateStyle>
            <IsSpecialLightEnabled>true</IsSpecialLightEnabled>
            <BrokenWindows Value="0,1" />
            <OpenedDoors Value="0" />
            <BrokenDoors Value="2" />
            <PoppedTyres Value="3" />
            <Neon Slot="1" On="true" />
            <EnginePowerMultiplier>1.1</EnginePowerMultiplier>
            <EngineTorqueMultiplier>0.8</EngineTorqueMultiplier>
            <TrimColor>4</TrimColor>
            <CustomPrimaryColor>
                <Red>100</Red>
                <Green>100</Green>
                <Blue>255</Blue>
            </CustomPrimaryColor>
            <CustomSecondaryColor>
                <Red>255</Red>
                <Green>55</Green>
                <Blue>34</Blue>
            </CustomSecondaryColor>
            <ModColor1>
                <Red>255</Red>
                <Green>0</Green>
                <Blue>0</Blue>
            </ModColor1>
            <ModColor2>
                <Red>0</Red>
                <Green>255</Green>
                <Blue>0</Blue>
            </ModColor2>
            <TyreSmokeColor>
                <Red>0</Red>
                <Green>255</Green>
                <Blue>0</Blue>
            </TyreSmokeColor>
            <NeonColor>
                <Red>20</Red>
                <Green>200</Green>
                <Blue>255</Blue>
            </NeonColor>
            <PearlescentColor>4</PearlescentColor>
            <WheelColor>12</WheelColor>
            <WheelType>4</WheelType>
            <DashboardColor>61</DashboardColor>
            <Health>0.2</Health>
            <Livery>12</Livery>
        </MapObject>
    </Objects>
</Map>
```

Element                 | Data Type     | Required  | Default   | Description
------------------------|---------------|-----------|-----------|-------------------------------------------------------
Type                    | String        | Yes       | *n/a*     | Must be `Vehicle` to be a vehicle entity.
Hash                    | Int32         | Yes       | *n/a*     | A valid hash value. The map loader will warn about invalid values.
TemplateName            | String        | No        | *n/a*     | Optional name of the template. Templates are not spawned but are used by other functionality to spawn a entity describes by the template.
Position                | Vector3f      | No        | 0,0,0     | Position of the entity in game world coordinates.
Rotation                | Vector3f      | No        | 0,0,0     | Rotation of the entity in game world coordinates.
PrimaryColor            | Int32         | No        | 0         | Primary vehicle color using the indexed color palette of GTA V.
SecondaryColor          | Int32         | No        | 0         | Secondary vehicle oclor using the index color palette of GTA V.
IsInvincible            | Bool          | No        | false     | Determine whether the vehicle is invincible and it is receiving damage or not.
IsCollisionless         | Bool          | No        | false     | Determines whether other entities will collide with this vehicle.
IsEngineRunning         | Bool          | No        | false     | Determines whether the vehicles engine is running.
HasBulletproofTyres     | Bool          | No        | false     | Sets whether the vehicle has bulletproof tyres.
IsPositionFrozen        | Bool          | No        | false     | Sets whether the vehicles position can change by either being drove or moved by another entity acting upon it.
NumberPlate             | String        | No        | *empty*   | Sets the vehicles license plate.
NumberPlateStyle        | Int           | No        | *empty*   | Sets the vehicles number plate style.
IsSpecialLightEnabled   | Bool          | No        | false     | Whether the vehicles special lights are enabled or not.
TrimColor               | Int32         | No        | *empty*   | Sets the trim color using the indexed color palette of GTA V.
BrokenWindows           | List<int>     | No        | *empty*   | A comma separated list of numbers of broken windows. e.g. 1,2,5 would specify the windows 1, 2 and 5 of the vehicle as broken/smashed.
OpenedDoors             | List<int>     | No        | *empty*   | A comma separated list of numbers of opened doors. e.g. 1,3 would specify the doors 1 and 3 as opened.
BrokenDoors             | List<int>     | No        | *empty*   | A comma separated list of number of broken doors. e.g. 2,1 would specify the doors 1 and 2 as broken.
PoppedTyres             | List<int>     | No        | *empty*   | A comma separated list of numbers of poppoed vehicle tyres. e.g. 2,3 would specify the tyres 2 and 3 as broken.
Neon                    | Neon (Element)| No        | *empty*   | A neon element containing none, one or more neon definitions for suitable vehicles. See the *neon* element for further information.
EnginePowerMultiplier   | Float         | No        | *empty*   | A value ranging from 0.0 to ~posinf~ 
EngineTorqueMultiplier  | Float
CustomPrimaryColor      | Color
CustomSecondaryColor    | Color
ModColor1               | Color
ModColor2               | Color
NeonColor               | Color
TyreSmokeColor          | Color
WheelColor              | Int32
WheelType               | Int32
WindowTint              | Int32
DashboardColor          | Int32
Health                  | Float
Livery                  | Int32
PearlescentColor        | Int32      


*Neon element*
*Color element*

Other common objects defined inside map files and synchronized with the client are `Markers` and `Blips`.

# Development
To be able to make changes to the project you need a .NET framework 4.6.1 installation and the msbuild toolset version 14.0. IDE does not matter that much but Visual Studio is recommended. For build and integration tasks `gulp` is used. This requires nodejs / npm.

* Clone repository
```
git clone https://github.com/jbuechner/gtmp-evilempire.git
```
* Install npm dependencies
```
npm install
```
* Build
```
gulp rebuild
```

## Targets
There are different *gulp* tasks available for development.

* *rebuild* (default)
  Deletes the *dist* directory, recompiles, retranspiles and copies all additional files again into the *dist* directory. This will lead to a new database.
* *build-resources*
  Copies the content of the `src/public` directory and creates the GTMP resource meta description XML file inside the *dist* directory.
  As the modification utilize a single resource concept this task is solely used to update the client side part of the modification.
* *build-cs*
  Compiles the server part CSharp sources and copies the binary output to the *dist* directory.
* *build*
  Builds CSharp and resources for the modification and copies them.
* *copy*
  Copies everything, the resource files, binaries, GTMP server redistributable and other static content like maps into the *distt* directory.
* *clean*
  Removes the *dist* directory and its contents.
  
  ## Scripts
  Common scripts usable by `npm` are either `cshash` to generate a pre-hashed client side password that can be used by the authentication implementation of the modification, or the `http:dev` script hosting a local http server to be able to test UI components of the modification.