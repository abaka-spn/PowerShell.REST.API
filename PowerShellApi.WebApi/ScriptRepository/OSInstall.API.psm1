#$script:LogFile = "C:\temp\OSInstall.log"

[String]$Script:MODULE_DIRECTORY = [System.IO.Path]::GetDirectoryName($myInvocation.MyCommand.Definition)
$script:LogFile = [System.IO.Path]::GetFullPath($Script:MODULE_DIRECTORY + "\" + [System.IO.Path]::GetFileNameWithoutExtension($myInvocation.MyCommand.Definition) + ".log")

<#
.Synopsis
New server in OSInstall process

.DESCRIPTION
Server creation logged in OSInstall.log file
This must be called at the end of Windows OS deployment

.PARAMETER Name
Name of the relating server

.EXAMPLE
    Add-OSInstallServer -Name "TADEOASWISZ01" 

.Notes
    Author: YASE Corporation
    Version: 1.0
    Last Edit: 05/04/2018 22:00
#>

Function Add-OSInstallServer{

    param(
		[ValidatePattern("^[\d\w]{13,14}$")]
        [Parameter(mandatory=$true)][string]$Name,
        [string]$UserName,
        [string]$Detail
    )

    $message = "{0}:{1}:{2}:{3}:{4}" -f $((get-date).ToString('yyyyMMdd-HHmm')), "ADD", ($Name| convertto-json), $UserName, $Detail
    Add-Content -Value $message -Path $script:LogFile -ErrorAction Stop
    "New server added successully"

}


<#
.Synopsis
Server deletion in OSInstall process

.DESCRIPTION
Server deletion logged in OSInstall.log file
This must be called at the deletion of a Windows server

.PARAMETER Name
Name of the relating server

.EXAMPLE
    Remove-OSInstallServer -Name "TADEOASWISZ01" 

.Notes
    Author: YASE Corporation
    Version: 1.0
    Last Edit: 05/04/2018 22:00
#>

Function Remove-OSInstallServer{

    param(
        [Parameter(mandatory=$true)][string]$Name
    )

    $message = "{0}:{1}:{2}" -f $((get-date).ToString('yyyyMMdd-HHmm')), "REMOVE", $Name
    Add-Content -Value $message -Path $script:LogFile -ErrorAction Stop
    "Server removed successully"

}

<#
.Synopsis
Get OS Install server events

.DESCRIPTION
Shows created and removed server events

.PARAMETER Name
Name of the server you want to query in log file

.EXAMPLE
    Get-OSInstallServer -Name "TADEOASWISZ01"

.Notes
    Author: YASE Corporation
    Version: 1.0
    Last Edit: 05/04/2018 22:00
#>
Function Get-OSInstallServer
{

    param(
        #PowerShellRestApi.Location=Path
        #PowerShellRestApi.Sample=TADEODCWISA01
        [Parameter(mandatory=$true)][string]$Name

    )

    $tab=@()
    $i=0

    try
    {
        Get-Content $script:LogFile -ErrorAction Stop | %{
            $event = $_ -split ":"
            New-Object -TypeName psobject -Property @{"Datetime"=$event[0];"Action"=$event[1];"Server"=$event[2];"CreatedBy"=$event[3]} | ? server -EQ $name | %{$i++;$_}
        }
    }
    catch
    {
        if( $_.CategoryInfo.Category -eq [System.Management.Automation.ErrorCategory]::ObjectNotFound -or
            $_.CategoryInfo.Category -eq [System.Management.Automation.ErrorCategory]::InvalidArgument -or
            $_.CategoryInfo.Category -eq [System.Management.Automation.ErrorCategory]::PermissionDenied)
        {
        #    Write-Error -Exception $_.Exception -Message "Not Fooound" -Category OpenError  -ErrorAction Stop
            throw $PSItem.Exception
        }
        else
        {
            throw $PSItem
        }
    }

    if($i -eq 0)
    {
        Write-Error "Server $Name not found in events" -Category ObjectNotFound
    }
}