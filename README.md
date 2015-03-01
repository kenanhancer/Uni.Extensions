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

//"DateTime@" value is not valid. So, result will be TypeCode.Empty
TypeCode dateTimeTypeCode_AsNotValid = "DateTime@".To<TypeCode>(TypeCode.Empty);

//"DateTime@" value is not valid. So, result will be null
TypeCode? dateTimeTypeCode_AsNotNullable = "DateTime@".To<TypeCode?>(null);

TypeCode booleanTypeCode_AsValid = "Boolean".To<TypeCode>();
```

####How to Convert DateTime string values to DateTime Types?

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

####How to Convert TimeSpan string values to TimeSpan Types?

```csharp

```

####How to Convert Primitive string values to Primitive Types?

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


