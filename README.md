###How To Install It?
Drop `UniExtensions` C#.NET code file into your project and change it as you wish or you can install from `NuGet Galery`;

If you want to install from `Nuget`, you should write Package Manager Console below code and `Uni.Extensions` will be installed automatically.
```
Install-Package Uni.Extensions
```
By the way, you can also reach `Uni.Extensions` `NuGet` package from http://nuget.org/packages/Uni.Extensions address.

Let's have a look at `Uni.Extensions` usages like below.

####How To Convert `Guid string` value to `Guid` Type?
Let's say we store guid values in database as string. When we want to use in `.Net` side, we should parse firstly.

```csharp
Guid guid_AsValid = "63ba52b5-1dd5-4b26-ba27-83f62c3d7e48".To<Guid>();

//As you can see Guid string has 3 more characters in the end(@), so this string
//can not be parsed to Guid Type. But, default value returned back.
Guid guid_AsNotValid = "63ba52b5-1dd5-4b26-ba27-83f62c3d7e48@@@".To<Guid>(Guid.Empty);

//Uni.Extensions is also appropriate for Nullable types. As you can see below code, 
//if Guid string is not valid, default value will be null.
Guid? guid_AsNullable = "63ba52b5-1dd5-4b26-ba27-83f62c3d7e48@@@".To<Guid?>(null);
```

####How To Convert `Enum string` value to `Enum` Type?

As an example, I use TypeCode Enum in System namespace.

```csharp
TypeCode dateTimeTypeCode_AsValid = "DateTime".To<TypeCode>();

//"DateTime@" value is not valid. So, result will be TypeCode.Empty
TypeCode dateTimeTypeCode_AsNotValid = "DateTime@".To<TypeCode>(TypeCode.Empty);

//"DateTime@" value is not valid. So, result will be null
TypeCode? dateTimeTypeCode_AsNotNullable = "DateTime@".To<TypeCode?>(null);

TypeCode booleanTypeCode_AsValid = "Boolean".To<TypeCode>();
```

####How to Convert `DateTime string` values to `DateTime` Types?

```csharp
DateTime dateTime = DateTime.Now.ToString().To<DateTime>();

DateTime dateTime2 = DateTime.Now.ToShortDateString().To<DateTime>();

DateTime dateTime3 = "3/1/2015 6:35:50 PM".To<DateTime>(DateTime.MinValue);

DateTime dateTime4 = "3/1/2015".To<DateTime>(DateTime.MinValue);

//"3/1/2015@" value is not valid. So, result will be DateTime.MinValue
DateTime dateTime5 = "3/1/2015@".To<DateTime>(DateTime.MinValue);

//"3/1/2015@" value is not valid. So, result will be null
DateTime? dateTime6 = "3/1/2015@".To<DateTime?>(null);
```

####How to Convert `TimeSpan string` values to `TimeSpan` Types?

```csharp
TimeSpan timeSpan = "6:35:50 PM".To<TimeSpan>();
```

####How to Convert `Primitive string` values to `Primitive` Types?

```csharp
int integer_AsValid = "25".To<int>(-1);

//"25j" value is not valid. So, result will be -1
int integer_AsNotValid = "25j".To<int>(-1);

//"25j" value is not valid. So, result will be null
int? integer_AsNullable = "25j".To<int?>(null);

double double1 = "30.36".To<double>();

float float1 = "45.21".To<float>();

decimal decimal1 = "4544343566".To<decimal>();

Single single1 = "32893".To<Single>();

bool bool1 = "True".To<bool>();

bool bool2 = "FALSE".To<bool>();

bool bool3 = "1".To<bool>();

bool bool4 = "0".To<bool>();

bool bool5 = "yes".To<bool>();

bool bool6 = "no".To<bool>();

bool bool7 = "off".To<bool>();

bool bool8 = "on".To<bool>();
```

####How to Convert `POCO` Type to `ExpandoObject`?

```csharp
public class Person
{
    public int PersonID { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string City { get; set; }
}

var person1 = new Person { PersonID = 1, FirstName = "Enes", LastName = "Hançer" }.To<ExpandoObject>();

var person2 = new Person { PersonID = 2, FirstName = "Sinan", LastName = "Hançer" }.ToExpando();
```

####How to Convert `Dictionary` Type to `ExpandoObject` or `POCO` Type Object?

```csharp
public class Person
{
    public int PersonID { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string City { get; set; }
}

//Person type has 4 properties which are PersonID, FirstName, LastName and City.
//We can create Dictionary which contains those properties. and than can convert to
//Person type easily.
var personDict = new Dictionary<string, object>()
{
    {"PersonID", 1},
    {"FirstName", "Kenan"},
    {"LastName", "Hançer"},
    {"City", "Istanbul"}
};

//Convert Dictionary to Person strongly type object.
Person personStronglyObj = personDict.To<Person>();

//Convert Dictionary to ExpandoObject
var personExpandoObject = personDict.ToExpando();
```

####How to Convert `object` to `Dictionary`?

```csharp
public class Person
{
    public int PersonID { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string City { get; set; }
}

var personObj = new Person { PersonID = 1, FirstName = "Enes", LastName = "Hançer" };

IDictionary<string, object> personToDict = personObj.ToDictionary();
```

####How to Convert `object` to `DataTable`?

```csharp
//Anonymous object to DataTable
var anonymousObj = new { PersonID = 1, FirstName = "Kenan", LastName = "Hançer", City = "İstanbul" };
DataTable dt1 = anonymousObj.ToDataTable();

//Strongly type to DataTable
var person1 = new Person { PersonID = 1, FirstName = "Enes", LastName = "Hançer" };
DataTable dt2 = person1.ToDataTable();

dynamic expandoObj = new ExpandoObject();
expandoObj.PersonID = 2;
expandoObj.FirstName = "Enes";
expandoObj.LastName = "Hançer";
expandoObj.City = "İstanbul";
expandoObj.Age = 1;

//ExpandoObject to DataTable
DataTable dt3 = (expandoObj as object).ToDataTable();

var customerList = new List<dynamic>{
    new { first_name = "kenan", last_name = "hancer", email = "kenanhancer@hotmail.com", active = true, store_id = 1, address_id = 5, create_date=DateTime.Now },
    new { first_name = "sinan", last_name = "hancer", email = "kenanhancer@hotmail.com", active = true, store_id = 1, address_id = 5, create_date=DateTime.Now },
    new { first_name = "kemal", last_name = "hancer", email = "kenanhancer@hotmail.com", active = true, store_id = 1, address_id = 5, create_date=DateTime.Now }
};

//List to DataTable
DataTable dt4 = customerList.ToDataTable();
```

####How to Convert `Collection` to `CSV`?

```csharp
var customerList = new List<dynamic>{
    new { first_name = "kenan", last_name = "hancer", email = "kenanhancer@hotmail.com", active = true, store_id = 1, address_id = 5, create_date=DateTime.Now },
    new { first_name = "sinan", last_name = "hancer", email = "kenanhancer@hotmail.com", active = true, store_id = 1, address_id = 5, create_date=DateTime.Now },
    new { first_name = "kemal", last_name = "hancer", email = "kenanhancer@hotmail.com", active = true, store_id = 1, address_id = 5, create_date=DateTime.Now }
};

string csvResult = customerList.WriteCsv<dynamic>(Encoding.Default);
```
