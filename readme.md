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
