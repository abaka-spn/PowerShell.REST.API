class Animal
{
    #[System.ComponentModel.DefaultValueAttribute("Dog")]
	[Parameter(HelpMessage="Breed of Animal",Mandatory=$true)]
	[string]$Breed

	[Parameter(HelpMessage="Name of animal")]
    [ValidateNotNullOrEmpty()]
    [string]$Name

	[Parameter(HelpMessage="Age of Animal (between 0 and 99)")]
    [ValidateRange(0,99)] #object minRange, object maxRange
    [int]$Age

	[Parameter(HelpMessage="Animal is alive or not")]
	[bool]$Alive

	[Parameter(HelpMessage="Number of leg (0, 2 or 4)")]
    [ValidateSet(0, 2,4)]
	[int]$Legs

	[Parameter(HelpMessage="List of nickname")]
    [AllowEmptyCollection()]
    [String[]]$Nickname

	[Parameter(HelpMessage="Codes of the name of country (ISO Alpha-2 or Alpha-3)")]
    [ValidateLength(2,3)] #int minLength, int maxlength
    [string]$CountryCode

	[Parameter(HelpMessage="National ID (Allow null)")]
    [AllowNull()]
	[string]$NationalId

	[Parameter(HelpMessage="Name of parents (Array with 1 or 2 values)")]
    [ValidateCount(1,2)] #int minLength, int maxlength
    [string[]]$Parents

	[Parameter(HelpMessage="InternalId (format: 2 alpha and 2 numeric)")]
    [ValidatePattern("^\w{2}\d{2}$")] #string regexString, Named Parameters
    [string]$InternalId

    [Parameter(HelpMessage="Private info (must be hidden in api)",DontShow)]
    [string]$PrivateInfo


    static [Animal]Example()
    {
        return [Animal]@{
            FirstName = "Sébastien"
            Age = 39
            ValueCount = @("a1","a2","a3","a4","a5")
            ValueLength = "abcdefgijk"
            ValuePattern = "20002949"
            ValueRange = 8
            ValueSet = "allow2"
        }
    }
}

<#
.Synopsis
Show message Synopsis

.DESCRIPTION
Show message description
suite de la description

.PARAMETER Breed
Filter by breed (from help section)

.PARAMETER Alive
Filter by alive (from help section)

.EXAMPLE
    Get-Animal -Breed "Dog"

.Notes
    Author: Sébastien PICHON
    Version: 1.0
    Last Edit: 31/1/2015 15:30
#>
Function Get-Animal
{
    #Description de la fonction 
	[OutputType([Animal[]])]
    Param
    (
        #Filter by breed
		[AllowNull()]
        [string]$Breed,

		#Filter by alive
        [switch]$Alive,

        [switch]$LoadIfNeeded

    )

    if ($LoadIfNeeded.IsPresent -and $Script:Animals.Count -eq 0)
    {
        Import-Animal -DbPath $Script:DbPath
    }

	if ($MyInvocation.BoundParameters.ContainsKey("Breed") -and $MyInvocation.BoundParameters.ContainsKey("Alive"))
	{
	    $Script:Animals | ? Breed -EQ $Breed | ? Alive -EQ $Alive.IsPresent
	}
	elseif ($MyInvocation.BoundParameters.ContainsKey("Breed"))
	{
	    $Script:Animals | ? Breed -EQ $Breed
	}
	elseif ($MyInvocation.BoundParameters.ContainsKey("Alive"))
	{
	    $Script:Animals | ? Alive -EQ $Alive.IsPresent
	}
    else
    {
	    $Script:Animals
    }
}

<#
.Synopsis
Create a new animal

.DESCRIPTION
Create a new animal for the demo 

.PARAMETER Name
Name of animal (from help section)

.Notes
    Author: Sébastien PICHON
    Version: 1.0
    Last Edit: 4/1/2018 15:30
#>
Function New-Animal
{
	Param
    (
		[System.ComponentModel.DefaultValueAttribute("Dog")]
		[Parameter(HelpMessage="Breed of Animal",Mandatory=$true)]
		[string]$Breed,

		[Parameter(HelpMessage="Name of animal")]
		[ValidateNotNullOrEmpty()]
		[string]$Name,

		[Parameter(HelpMessage="Age of Animal (between 0 and 99)")]
		[ValidateRange(0,99)] #object minRange, object maxRange
		[int]$Age,

		[Parameter(HelpMessage="Animal is alive or not")]
		[bool]$Alive,

		[Parameter(HelpMessage="Number of leg (0, 2 or 4)")]
		[ValidateSet(0, 2,4)]
		[int]$Legs,

		[Parameter(HelpMessage="List of nickname")]
		[AllowEmptyCollection()]
		[String[]]$Nickname,

		[Parameter(HelpMessage="Codes of the name of country (ISO Alpha-2 or Alpha-3)")]
		[ValidateLength(2,3)] #int minLength, int maxlength
		[string]$CountryCode,

		[Parameter(HelpMessage="National ID (Allow null)")]
		[AllowNull()]
		[string]$NationalId,

		[Parameter(HelpMessage="Name of parents (Array with 1 or 2 values)")]
		[ValidateCount(1,2)] #int minLength, int maxlength
		[string[]]$Parents,

		[Parameter(HelpMessage="InternalId (format: 2 alpha and 2 numeric)")]
		[ValidatePattern("^\w{2}\d{2}$")] #string regexString, Named Parameters
		[string]$InternalId,

		[Parameter(HelpMessage="Private info (must be hidden in api)",DontShow)]
		[string]$PrivateInfo
	)

	[Animal]$Animal = New-Object Animal -Property @{
												Breed = $Breed
												Name = $Name
												Age = $Alive
												Alive = $true
												Legs = $Legs
												Nickname = $Nickname
												CountryCode = $CountryCode
												NationalId = $NationalId
												Parents = $Parents
												InternalId = $InternalId
												PrivateInfo = $PrivateInfo
											}

    if ($Script:Animals -eq $null)
    {
        $Script:Animals = @()
    }

	$Script:Animals += $Animal

	Save-Animal -DbPath $Script:DbPath

    @{
		Message = "[NEW OK] The name of animal is $Name." ;
		Code = 0
	}
}

<#
.Synopsis
dd a new animal from class

.DESCRIPTION
Add a new animal from Animal class defined in PowerShell Script

.PARAMETER Animal
Animal object (from help section)

.Notes
    Author: Sébastien PICHON
    Version: 1.0
    Last Edit: 4/1/2018 15:30
#>
Function Add-Animal
{
	Param
    (
		[Parameter(HelpMessage="Animal object",Mandatory=$true)]
		[Animal]$Animal
	)

    if ($Script:Animals -eq $null)
    {
        $Script:Animals = @()
    }

	$Script:Animals += $Animal

	Save-Animal -DbPath $Script:DbPath

    @{
		Message = "[NEW OK] The name of animal is $Animal.Name." ;
		Code = 0
	}
}


<#
.Synopsis
Load sample animals for demonstration

.DESCRIPTION
Load static animals for demonstration

#>
Function Import-Animal
{
	Param
    (
		[string]$DbPath
	)
	if (-Not [string]::IsNullOrWhiteSpace($DbPath))
	{
		$Script:Animals = Get-Content $DbPath | ConvertFrom-Json
	}
	elseif ($Script:Animals.count -eq 0)
	{

		$Script:Animals += New-Object Animal -Property @{
													Breed = "Dog"
													Name = "Medor"
													Age = 10
													Alive = $true
													Legs = 4
													Nickname = @("Memed")
													CountryCode = "FRA"
													NationalId = $null
													Parents = @("Croquette","Pilou")
													InternalId = "FR01"
													PrivateInfo = "My dog"
												}

		$Script:Animals += New-Object Animal -Property @{
													Breed = "Dog"
													Name = "Ralph"
													Age = 13
													Alive = $false
													Legs = 4
													Nickname = $null
													CountryCode = "US"
													NationalId = $null
													Parents = @("Nancy","Nascar")
													InternalId = "US01"
													PrivateInfo = "By Francois"
												}

		$Script:Animals += New-Object Animal -Property @{
													Breed = "Cat"
													Name = "Minouchette"
													Age = 3
													Alive = $true
													Legs = 4
													Nickname = @("chouchou")
													CountryCode = "BE"
													NationalId = $null
													Parents = @("Matou","Ragga")
													InternalId = "US01"
													PrivateInfo = "By Yann"
												}

		$Script:Animals += New-Object Animal -Property @{
													Breed = "Tiger"
													Name = "Tony"
													Age = 28
													Alive = $true
													Legs = 4
													Nickname = @("Tigrounet")
													CountryCode = "BN"
													NationalId = "TIG012"
													Parents = @("Chenca","Raf")
													InternalId = "BG01"
													PrivateInfo = "By Laurent"
												}

		$Script:Animals += New-Object Animal -Property @{
													Breed = "Chicken"
													Name = "KotteKotte"
													Age = 1
													Alive = $true
													Legs = 2
													Nickname = @("MonPoulet")
													CountryCode = "CH"
													NationalId = "CHK012"
													Parents = @("Coutcout","Cutcut")
													InternalId = "AN01"
													PrivateInfo = "By Francois"
												}
    }
}

<#
.Synopsis
Save DB to Json file

.DESCRIPTION
Save DB to Json file

#>
Function Save-Animal
{
	Param
    (
		[string]$DbPath
	)

	if ($Script:Animals.count -ne 0 -and -Not [string]::IsNullOrWhiteSpace($DbPath))
	{
		$Script:Animals | ConvertTo-Json | Out-File $DbPath
	}
	

}

#Get-Variable Animals -Scope Script -ErrorAction SilentlyContinue | Remove-Variable
[Animal[]]$Script:Animals = @()

$Script:DbPath = [io.Path]::Combine([io.path]::GetDirectoryName($MyInvocation.MyCommand.path),"Animals-DB.json");
if (-Not (Test-Path $Script:DbPath))
{
	Import-Animal
	Save-Animal -DbPath $Script:DbPath
}
else
{
		Import-Animal -DbPath $Script:DbPath
}

Write-Host ("{0} animals" -f $Script:Animals.Count)




Function Get-Info
{
    #Description de la fonction 
    Param
    (
        [string]$Message,

		[string]$User = "DemoUser",

		[string[]]$Roles = @("DemoRole1","DemoRole2"),


		[System.Security.Claims.Claim[]]$Claims = (new-object System.Security.Claims.Claim([System.Security.Claims.ClaimTypes]::Name,"Anonymous"))
    )


	(New-Object Animal -Property @{Name = $User; "Nickname" = $Roles; Parents=($Claims | % {$_.Value})})

    if($User -eq "SeB2")
    {
        $E = new-object System.Exception
        Write-Error -Exception $E -Message "Vous n'etes pas autorisé a réaliser cette tache." -Category ObjectNotFound

        #$Error[0].Exception -is [System.Exception]
        #$Error[0] | ConvertTo-Json 

    }

}