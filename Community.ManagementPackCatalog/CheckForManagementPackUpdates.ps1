# CheckForManagementPackUpdates.ps1 is part of the Community Management Pack Catalog and licensed GPL v3; See https://github.com/mpcatalog/scom-community-catalog-ui/blob/master/LICENSE.

# Import the OpsMgr Module and connect to localhost this will target
# the RMS Emulator so we know it will be a Management Server

Import-Module OperationsManager
New-SCOMManagementGroupConnection localhost

# Create a API object which will be used to report the status back to the monitor
$api = New-Object -Com MOM.ScriptAPI
$bag = $api.CreatePropertyBag()

# Use the redirect to pull out base repository location
$redirectBaseUrl = "http://www.mpcatalog.net/CatalogRepo";
$gitHubRepoBase = (Invoke-WebRequest -Uri $redirectBaseUrl -MaximumRedirection 0 -ErrorAction SilentlyContinue -Headers @{"Referer"="Http://Ps.UpdateMonitor"}).Headers.Location;

# Test to see if this is a secondary redirect, we will follow a maximum of two
$redirectTestResult = (Invoke-WebRequest -Uri $redirectBaseUrl -MaximumRedirection 0 -ErrorAction SilentlyContinue -Headers @{"Referer"="Http://Ps.UpdateMonitor"})
if ($redirectTestResult.StatusCode -eq 301 -or $redirectTestResult.StatusCode -eq 302)
	{
		# If this is another redirect, we'll update the repo address.  If not, do nothing.
		$gitHubRepoBase = $redirectTestResult.Headers.Location
	}

$managementPackIndexUrl = $gitHubRepoBase + "Index.json";

# Get list of management packs to work with
$mpsFromGitHub = Invoke-RestMethod -Uri $managementPackIndexUrl -Method Get;

[bool]$OutdatedPacks = $false;
$StatusMessage = [string]::Empty

# Check each management pack in the list for status
foreach($mp in $mpsFromGitHub)
    {
		# Get the details from GitHub for this specific pack.
        $detailsJsonFileLocation = $gitHubRepoBase + $mp.ManagementPackSystemName + "/details.json";
        $PackDetails = Invoke-RestMethod -Uri $detailsJsonFileLocation -Method Get | Select ManagementPackSystemName,@{n="Version"; e={[version]::Parse($_.Version)}},ManagementPackDisplayName
        
		# Pull the management pack via SDK, with $null as the fall back
		$InstalledMp = $null
        $InstalledMp = Get-SCOMManagementPack -Name $PackDetails.ManagementPackSystemName

		# If the pack is installed, compare the version against GitHub.  For packs with updates available, add to the OutdatedPacks counter.
		If($InstalledMp -ne $null)
            {
                If($PackDetails.Version -gt $InstalledMp.Version)
                    {
						$PackCheckMessage = "Performed version check on {0}, found {1} in the catalog and {2} in the management group.  The result was: Outdated" -f $PackDetails.ManagementPackSystemName,$PackDetails.Version.ToString(),$InstalledMp.Version.ToString()
                        $api.LogScriptEvent("CheckForManagementPackUpdates.ps1",4501,0,$PackCheckMessage)

						#You don't have the latest version
                        $OutdatedPacks = $true
						$StatusMessage = "{0} `n {1}: {2}" -f $StatusMessage, $PackDetails.ManagementPackSystemName, "Update available" 
                    }
				else
					{
						$PackCheckMessage = "Performed version check on {0}, found {1} in the catalog and {2} in the management group.  The result was: Up-To-Date" -f $PackDetails.ManagementPackSystemName,$PackDetails.Version.ToString(),$InstalledMp.Version.ToString()
						$api.LogScriptEvent("CheckForManagementPackUpdates.ps1",4500,0,$PackCheckMessage)
					}
            }
    }

# Create the response
$bag = $api.CreatePropertyBag()

if($OutdatedPacks)
    {
        # Time To Alert
        $bag.AddValue('status', 'Outdated')
    }
else
    {
        # Things look good
        $bag.AddValue('status', 'UpToDate')
    }

$bag.AddValue('PackStatus',$StatusMessage)

$bag