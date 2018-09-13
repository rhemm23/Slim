# Slim
Light Cassandra ORM written around the datastax c# driver

### Setup / Configuration

To configure the ORM to use your ISession instance and cluster, simply set the GlobalConfiguration.

```cs
Cluster cluster = Cluster.Builder().AddContactPoint("127.0.0.1").Build();
ISession session = cluster.Connect();

GlobalConfiguration.SetConfiguration(new CassandraORM.Configuration()
{
    UseBindings = true,
    Cluster = cluster,
    Session = session
});
```

UseBindings is a configuration option added to indicate whether or not the query should print out the literal value of constants,
or add a parameter bind marker. In general, it is more appropriate to use parameter bind markers.

Any classe that will be used with the ORM needs to have the proper attributes. Each class definition must have a 'Table' attribute, to define the keyspace and table. A keyspace name must be provided, however the table name is optional. If a table name is not provided, it will be infered from the class name. Class properties can have a 'Column' attribute, which will allow for you to specify the column name. If a column attribute is not supplied, the column name will be infered from the property name. All class properties will be interpreted as columns unless an 'Ignore' attribute is supplied.

```cs
[Table("UserData")]
public class User
{
  [Column(Name = "username")]
  public string Username { get; set; }

  [Column(Name = "email")]
  public string Email { get; set; }

  [Column(Name = "first_name")]
  public string FirstName { get; set; }

  [Column(Name = "last_name")]
  public string LastName { get; set; }
  
  [Ignore]
  public string FullName => $"{FirstName} {LastName}";
}
```

### Select Builder

The select builder builds a query from a series of expressions. The result can be built into query text, or executed to retrieve data.
Execution can be done asynchronously or synchronously. 

```cs
User user = new SelectBuilder<User>()
                    .Where(u => u.FirstName == "Bob")
                    .FirstOrDefault();
```

Multiple rows of information can be retrieved as well, in the form of a List.

```cs
List<User> users = await new SelectBuilder<User>()
                    .Where(u => u.IsEmailVerified == true)
                    .ToListAsync();
```

### Update Builder

The update builder will build a query to update information. Many features of Cassandra are supported by the ORM, including lightweight
transactions, and updating collections.

```cs
bool executed = new UpdateBuilder<User>()
                    .Set(u => u.FirstName, "Joe")
                    .Where(u => u.FirstName == "Bob")
                    .IfExists()
                    .Execute();
```

Here is an example of adding information to a set

```cs
HashSet<string> newNumbers = new HashSet<string>();

newNumbers.Add("555-555-5555");
newNumbers.Add("111-111-1111");

bool executed = new UpdateBuilder<User>()
        .AddToSet(u => u.PhoneNumbers, newNumbers)
        .IfExists()
        .Execute();
```

### Delete Builder

The delete builder will build a cassandra delete query. Again, lightweight transactions are supported.

```cs
bool executed = new DeleteBuilder<User>()
                    .Where(u => u.FirstName == "Bob")
                    .Execute();
```

Here is an example of a query deleting specific columns of information. 

```cs
bool executed = new DeleteBuilder<User>()
                    .Where(u => u.FirstName == "Bob")
                    .Delete(
                        u => u.LastName,
                        u => u.ProfilePicture)
                    .If(u => u.IsEmailVerified == true)
                    .Execute();
```

### Insert Builder

Similar to the other builders, the insert builder will build an insert query. Here TTL and Timestamp are supported.

```cs
bool executed = new InsertBuilder<User>()
                    .UseTTL(50)
                    .Insert(new User()
                    {
                        FirstName = "John",
                        LastName = "Doe"
                    });
```
