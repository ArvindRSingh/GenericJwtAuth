# ASP.NET Core JWT Authentication and Authorization independent of database
* Generates Json Web Tokens(JWT) authentication tokens and uses it to authenticate and authorize users regardless of underlying database used. Default ASP.NET API template uses Entity Framework and is tightly coupled with it.
* Use Azure Table Storage or can be made to use any other storage mechanism as primary database for storing users and roles.
 
# Problem statement
Entity framework either could not or was too complex to be tied up with any given database storage specially No-SQL types. Wanted a JWT generator that didn't use EF Identity.
 
# Thanks to
Discussion that helped the most  
https://github.com/dotnet/aspnetcore/issues/2193

Thanks to Lucas Simas for below repository for reference involved in above discussion  
https://github.com/lucassklp/Default.Architecture 

Decoding Json Web Tokens  
http://calebb.net

Add multiple role claims to JWT   
https://stackoverflow.com/a/49241043/219615 Pier-Lionel Sgard
