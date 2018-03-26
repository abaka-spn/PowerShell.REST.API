class RequestItem
{
    [string]$Source

    [string]$Destination

    #List http, tcp, icmp
    [string]$Protocol

    [Parameter(HelpMessage="Port (Format 80, 50-65)")]
    [ValidatePattern("^\d{2}$")] 
    [string]$Port

    #IP/FQDN(Alias)/User/Subnet(VLAN/VxLAN)
    [string]$SourceType

    #IP/FQDN(Alias)/Subnet(VLAN/VxLAN)
    [string]$DestinationType

    #Number/Range/Group
    [string]$PortType

    #Allowed/Denied/Unknown
    $ValadationState

    #Never(null)/Date
    $ExpirationDate
}

class Request
{
    [string] $Requester

    [int]$RequestId

    [RequestItem[]] $Items

    [Datetime]$LastValidate

    [Datetime]$SubmitDate
}


<#
.Synopsis
Demande une validation de la demande

.DESCRIPTION
Soumet pour validation la demande

.PARAMETER Requester
Nom du demandeur

.PARAMETER RequestId
Numero de la demande

.PARAMETER Items
Liste des demandes

.PARAMETER LastValidate
Date de la dernière vérification

.PARAMETER SubmitDate
Date de la soumission


.Notes
    Author: Sébastien PICHON

#>
Function Test-Request
{
	param(
		[string] $Requester,

		[int]$RequestId,

	    [ValidateCount(1,2)] #int minLength, int maxlength
		[RequestItem[]] $Items,

		[RequestItem] $Item,

		[Datetime]$LastValidate,

		[Datetime]$SubmitDate
	)
	
}

<#
.Synopsis
Soumet la demande

.DESCRIPTION
Soumet la demande

.PARAMETER Requester
Nom du demandeur

.PARAMETER RequestId
Numero de la demande

.PARAMETER Items
Liste des demandes

.PARAMETER LastValidate
Date de la dernière vérification

.PARAMETER SubmitDate
Date de la soumission


.Notes
    Author: Sébastien PICHON

#>
Function Send-Request
{
	Param (
		[string] $Requester,

		[int]$RequestId,

		[RequestItem[]] $Items,

		[Datetime]$LastValidate,

		[Datetime]$SubmitDate
	)
}



