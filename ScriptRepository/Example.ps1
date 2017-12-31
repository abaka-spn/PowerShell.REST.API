<#
.SYNOPSIS
	This script is PowerShell Script Sample for Rest Api exposition.
.DESCRIPTION
    This script content differents types arguments with attributes validator.
.NOTES
    Additional Notes, eg
    File Name  : Example.ps1
    Author     : Sébastien Pichon - sebastien.pichon@adeo.com
.LINK
    A hyper link, eg
.PARAMETER MESSAGE1
    Message 1 description
.PARAMETER MESSAGE2
	Message 2 description
#>
param ( 
	#Message 1 ok?
	[string]$Message1 = "def1",
	[string]$Message2 = "def2",
	[string]$Message3 = "def3",
	[string]$Message4 = "def4"
	)

#Sleep -s 10

@{
	forlder = Get-Item -Path . | Select-Object name;
	path = $PWD.path;
	msg1 = $Message1;
	msg2 = $Message2;
	msg3 = $Message3;
	msg4 = $Message4
}
