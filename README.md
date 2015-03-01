###How To Install It?
Drop Uni C#.NET code file into your project and change it as you wish or you can install from NuGet Galery;

If you want to install from Nuget, you should write Package Manager Console below code and Uni will be installed automatically.
```
Install-Package Uni.Extensions
```
By the way, you can also reach Uni NuGet package from http://nuget.org/packages/Uni.Extensions address.

Let's have a look at Uni.Extensions usages like below.

####How To Convert Guid string value to Guid Type?
Let's say we store guid values in database as string. When we want to use in .Net side, we should parse firstly.

```csharp
Guid guid_AsValid = "63ba52b5-1dd5-4b26-ba27-83f62c3d7e48".To<Guid>();

//As you can see Guid string has 3 more characters in the end(@), so this string
//can not be parsed to Guid Type. But, default value returned back.
Guid guid_AsNotValid = "63ba52b5-1dd5-4b26-ba27-83f62c3d7e48@@@".To<Guid>(Guid.Empty);

//Uni.Extensions is also appropriate for Nullable types. As you can see below code, 
//if Guid string is not valid, default value will be null.
Guid? guid_AsNullable = "63ba52b5-1dd5-4b26-ba27-83f62c3d7e48@@@".To<Guid?>(null);
```

####How To Convert Enum string value to Enum Type?

As an example, I use TypeCode Enum in System namespace.

```csharp
TypeCode dateTimeTypeCode_AsValid = "DateTime".To<TypeCode>();

TypeCode dateTimeTypeCode_AsNotValid = "DateTime@".To<TypeCode>(TypeCode.Empty);

TypeCode? dateTimeTypeCode_AsNotNullable = "DateTime@".To<TypeCode?>(null);

TypeCode booleanTypeCode_AsValid = "Boolean".To<TypeCode>();
```

####How to Convert DateTime string values to DateTime Types?

```csharp
DateTime dateTime = DateTime.Now.ToString().To<DateTime>();

DateTime dateTime2 = DateTime.Now.ToShortDateString().To<DateTime>();
```

####How to Convert TimeSpan string values to TimeSpan Types?

```csharp

```

##How to Convert Primitive string values to Primitive Types?

```csharp
int integer_AsValid = "25".To<int>(-1);

int integer_AsNotValid = "25j".To<int>(-1);

int? integer_AsNullable = "25j".To<int?>(null);
```


