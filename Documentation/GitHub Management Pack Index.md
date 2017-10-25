# Managing the GitHub Management Pack Index

*The list of Management Packs available through the Community Management Pack Catalog is managed via a GitHub index file.  The below documentation and steps cover best practices to get the most out of the Community Catalog with the least effort.*

## Overview
When the Community Management Pack Catalog view is loaded in the SCOM console the first operation is to pull the catalog index file via HTTP GET.  This file is used for the following functions.

* Indicates the list of packs that are *Active* and should be loaded
* Provides the specific URL to the Management Pack Detail JSON
* Serves as an easy to read and modify list of what packs the catalog holds


## Index JSON structure
```json
{ 
    "ManagementPackSystemName":"Community.ManagementPackCatalog",
    "Active":"true"
}
```

## Property Descriptions

### ManagementPackSystemName
The System Name of the Management Pack to be entered into the catalog.  The Management Pack Catalog UI will perform an HTTP GET to pull details.json from a sub-folder in this Repo.
This value must be unique within the index and should describe the pack.

### Active (Optional)
This property is optional and will default to true if no value is provided.  If set to false the UI will ignore this entry in the Management Pack Catalog.
