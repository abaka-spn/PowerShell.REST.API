$script:LogFile = "C:\temp\clap.log"

<#
.Synopsis
New server event in Clap

.DESCRIPTION
Server creation or deletion logged in event viewer (logname Application, event 66)
Server creation or deletion logged in clap.log file

.PARAMETER Name
Name of the relating server

.PARAMETER Action
Action done : ADD or REMOVE

.EXAMPLE
    New-ClapServer -Name "TADEOASWISZ01" -Action ADD

.Notes
    Author: Yann LECOUTY
    Version: 1.0
    Last Edit: 05/04/2018 22:00
#>

Function New-ClapServerEvent{

    param(
        [Parameter(mandatory=$true)][string]$Name,
        [Parameter(mandatory=$true)][ValidateSet("ADD","REMOVE")][string]$Action
    )

    $message = "{0}:{1}:{2}" -f $((get-date).ToString('yyyyMMdd-HHmm')), $Action, $Name

    try{
        Write-EventLog -LogName Application -Source "SceCli" -EntryType Information -EventId 66 -Message $message -ErrorAction Stop
        Add-Content -Value $message -Path $script:LogFile -ErrorAction Stop

        "OK"

    }catch{
        $_.exception.message
    }
}



<#
.Synopsis
Get clap server events

.DESCRIPTION
Shows created and removed server logged in clap.log file

.PARAMETER Name
Name of the server you want to query in log file

.PARAMETER All
Query every server in log file

.EXAMPLE
    Get-ClapServerEvents -Name "TADEOASWISZ01"

.Notes
    Author: Yann LECOUTY
    Version: 1.0
    Last Edit: 05/04/2018 22:00
#>


Function Get-ClapServerEvents{

    param(
        [Parameter(mandatory=$true)][string]$Name

    )

    $tab=@()

    (Microsoft.PowerShell.Management\Get-Content $script:LogFile) | %{
        $event = $_ -split ":"
        $tab+=New-Object -TypeName psobject -Property @{"Datetime"=$event[0];"Action"=$event[1];"Server"=$event[2]}
    }

    if($all.IsPresent -and $tab -ne $null){
        $tab | Microsoft.PowerShell.Management\Sort-Object Datetime
    }elseif($name){
        $res = $tab | ? server -EQ $name | Microsoft.PowerShell.Management\Sort-Object Datetime
        if(!$res){"No content in history for $name"}
        else{$res}
    }else{ "No content in history for any servers"}

}