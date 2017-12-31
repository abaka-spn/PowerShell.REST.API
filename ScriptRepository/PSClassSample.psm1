#https://kevinmarquette.github.io/2017-02-19-Powershell-custom-attribute-validator-transform/
Class OpenApi : Attribute {
    [string]$Examples

    MyCommand([string]$Examples)
    {
        $this.Examples = $Examples
    }
}



#https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_functions_advanced_parameters?view=powershell-5.1

#Ne fonctionne pas dans le meme module. Nécessite de charger l'attribut avec la class
#[OpenApi("OK")]

class Person
{
    [System.ComponentModel.DefaultValueAttribute("SeB")]
	[Parameter(HelpMessage="Donne ton prénom",Mandatory=$true)]
    [string]$FirstName

    [AllowEmptyString()]
    [string]$FirstName2 = "prenom par defaut"
    
    [AllowNull()]
    [int]$Age

    [AllowEmptyCollection()]
    [String[]]
    $ComputerName

    [ValidateCount(3,5)] #int minLength, int maxlength
    [string[]]$ValueCount

    [ValidateLength(8,10)] #int minLength, int maxlength
    [string]$ValueLength

    [ValidatePattern("^\d{8}$")] #string regexString, Named Parameters
    [string]$ValuePattern

    [ValidateRange(5,10)] #object minRange, object maxRange
    [int]$ValueRange

    [ValidateSet("allow1","allow2","allow3")] #params string[] validValues, Named Parameters
    [string]$ValueSet

    [ValidateNotNullOrEmpty()]
    [string]$NotNullOrEmpty

    [switch]$switch

    [Parameter(DontShow)]
    [string]$DontShow

    [string]
    hidden $Hidden

    static [Person]Sample()
    {
        return [Person]@{
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

.PARAMETER onePerson
bla bla sur le paramètre onePerson
suite du bla bla

.PARAMETER Allow
bla bla sur le paramètre Allow
suite du bla bla


.EXAMPLE
    Test-Demo -TonNom "ll"

.Notes
    Author: Sébastien PICHON
    Version: 1.0
    Last Edit: 31/1/2015 15:30
#>
Function Test-Demo
{
    #Description de la fonction 
    [Parameter(HelpMessage="c'est fonction de test")]
	[OutputType([Person])]
	[OutputType([string])]
    Param
    (
        #Description du param oneperson
        #Ligne 2 Description du param oneperson
        [Person]$onePerson = [Person]::Sample(),

		[Parameter(HelpMessage="Donne ton prénom")]
        [string]$TonNom,

		# Merci de me donner ton matricule sur 8 caractères
		[Parameter(HelpMessage="Donne moi ton matricule sur 8 caractères")]
	    [ValidatePattern("^\d{8}$")] #string regexString, Named Parameters
        [string]$Matricule,

        [Parameter(Mandatory=$false)]
        [ValidateSet("allow1","allow2","allow3")]
        [string]$Allow = "allow1",

        [Parameter(Mandatory=$false)]
        [ValidateSet(1)]
        [int]$AllowInt,


        [string[]]$AnimalsList = @("dog","cat"),

        #bla bla sur le paramètre Date non déclaré dans le Synopsis
        #suite du bla bla
        [ValidateScript({$_ -ge (get-date)})]
        [DateTime]$Date,

        [Parameter(Mandatory=$false)]
        [string]$toto
    )

    $onePerson.ValueSet = $Allow
    #("Bonjour " + $onePerson.FirstName + " - " + $onePerson.ValueSet)
    $onePerson
}

Function Test-Demo2
{
    "OK"
}
#[Person]::Example() | ConvertTo-Json
#Demo -Allow "allow1" | ConvertTo-Json


