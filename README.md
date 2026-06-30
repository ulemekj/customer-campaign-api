# Customer Campaign API project

## Prerequisites
Before setting up the project, ensure you have the following installed:
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (or the version your project targets)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) (with *ASP.NET and web development* & *.NET desktop development* workloads)


## Database setup (SQL Server LocalDB)

The project uses **Microsoft SQL Server LocalDB**, which comes bundled with Visual Studio. Follow these precise steps to configure and run the database:

### 1. Configure the Connection String
Open `appsettings.json` in the root of the API project and ensure your connection string points to your local LocalDB instance:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=CustomerRewardDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```
### 2. Apply database migrations
The initial migration files are already included in the repository. You do not need to create a new migration. Instead, open the **Package Manager Console** in Visual Studio and run the following command:   
Update-Database

To verify the tables, open View -> SQL Server Object Explorer in Visual Studio and expand the (localdb)\MSSQLLocalDB node  
Navigate to Databases -> CustomerRewardDb -> Tables -> CustomerRewards  
Right-click the table and select View Data to track records in real-time  

### 3. Run and test the project
Press F5 to run the project  
The browser will automatically open the Swagger UI where you can test the endpoints
