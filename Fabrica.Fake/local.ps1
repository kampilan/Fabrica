Import-Module Fabrica.One/Builder

clear

dotnet publish E:\repository\fabrica\Fabrica.Fake -c:Release -p:PublishProfile=ForRelease

New-Appliance -Name fabrica-fake -Build local -Source E:\repository\fabrica\Fabrica.Fake\bin\Release\net6.0\publish -Region us-east-2 -Bucket pondhawk-appliance-repository -GenerateLatest $false


