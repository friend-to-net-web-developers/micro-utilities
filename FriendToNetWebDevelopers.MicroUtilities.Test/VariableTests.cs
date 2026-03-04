using System.Reflection;
using FriendToNetWebDevelopers.MicroUtilities.Enum;

namespace FriendToNetWebDevelopers.MicroUtilities.Test;

public class MyTestClass { public int MyProperty { get; set; } }
public class MyGenericClass<T> { }

[TestFixture]
public class VariableTests
{
    [Test]
    public void Get_ShouldIdentifyCorrectType()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Utilities.Variable.GetVariableFormat("camelCase"), Is.EqualTo(ResultsVariableNameTypeEnum.CamelCase));
            Assert.That(Utilities.Variable.GetVariableFormat("PascalCase"), Is.EqualTo(ResultsVariableNameTypeEnum.PascalCase));
            Assert.That(Utilities.Variable.GetVariableFormat("snake_case"), Is.EqualTo(ResultsVariableNameTypeEnum.SnakeCase));
            Assert.That(Utilities.Variable.GetVariableFormat("SCREAMING_SNAKE_CASE"), Is.EqualTo(ResultsVariableNameTypeEnum.ScreamingSnakeCase));
            Assert.That(Utilities.Variable.GetVariableFormat("kebab-case"), Is.EqualTo(ResultsVariableNameTypeEnum.KebabCase));
            Assert.That(Utilities.Variable.GetVariableFormat("Train-Case"), Is.EqualTo(ResultsVariableNameTypeEnum.TrainCase));
            Assert.That(Utilities.Variable.GetVariableFormat("unicase"), Is.EqualTo(ResultsVariableNameTypeEnum.Unicase));
            Assert.That(Utilities.Variable.GetVariableFormat("TROLLCASE"), Is.EqualTo(ResultsVariableNameTypeEnum.TrollCase));
        });
    }

    [Test]
    public void Get_ShouldReturnUnknownForAmbiguousOrInvalid()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Utilities.Variable.GetVariableFormat(""), Is.EqualTo(ResultsVariableNameTypeEnum.Unknown));
            Assert.That(Utilities.Variable.GetVariableFormat(null!), Is.EqualTo(ResultsVariableNameTypeEnum.Unknown));
        });
    }

    [Test]
    public void Get_ShouldIdentifyWordsType()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Utilities.Variable.GetVariableFormat("hello world"), Is.EqualTo(ResultsVariableNameTypeEnum.Words));
            Assert.That(Utilities.Variable.GetVariableFormat("hello-world-123!"), Is.EqualTo(ResultsVariableNameTypeEnum.Words));
            Assert.That(Utilities.Variable.GetVariableFormat("_a_"), Is.EqualTo(ResultsVariableNameTypeEnum.Words));
            Assert.That(Utilities.Variable.GetVariableFormat("a-"), Is.EqualTo(ResultsVariableNameTypeEnum.Words));
            Assert.That(Utilities.Variable.GetVariableFormat("my_Variable"), Is.EqualTo(ResultsVariableNameTypeEnum.Words));
            Assert.That(Utilities.Variable.GetVariableFormat("my--variable"), Is.EqualTo(ResultsVariableNameTypeEnum.Words));
            Assert.That(Utilities.Variable.GetVariableFormat("123variable"), Is.EqualTo(ResultsVariableNameTypeEnum.Words));
            Assert.That(Utilities.Variable.GetVariableFormat("This Is A Title"), Is.EqualTo(ResultsVariableNameTypeEnum.Words));
            Assert.That(Utilities.Variable.GetVariableFormat("Sentence words"), Is.EqualTo(ResultsVariableNameTypeEnum.Words));
        });
    }

    [Test]
    public void Get_ShouldReturnUnknownForNoAlphanumeric()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Utilities.Variable.GetVariableFormat("_"), Is.EqualTo(ResultsVariableNameTypeEnum.Unknown));
            Assert.That(Utilities.Variable.GetVariableFormat("__"), Is.EqualTo(ResultsVariableNameTypeEnum.Unknown));
            Assert.That(Utilities.Variable.GetVariableFormat("-"), Is.EqualTo(ResultsVariableNameTypeEnum.Unknown));
            Assert.That(Utilities.Variable.GetVariableFormat("--"), Is.EqualTo(ResultsVariableNameTypeEnum.Unknown));
            Assert.That(Utilities.Variable.GetVariableFormat("!!!"), Is.EqualTo(ResultsVariableNameTypeEnum.Unknown));
        });
    }

    [Test]
    public void ConvertFromWords_ShouldWork()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Utilities.Variable.ConvertToCamelCase("hello world").result, Is.EqualTo("helloWorld"));
            Assert.That(Utilities.Variable.ConvertToPascalCase("hello-world-123!").result, Is.EqualTo("HelloWorld123"));
            Assert.That(Utilities.Variable.ConvertToSnakeCase("_a_").result, Is.EqualTo("a"));
            Assert.That(Utilities.Variable.ConvertToCamelCase("HelloWorld!").result, Is.EqualTo("helloWorld"));
        });
    }

    [Test]
    public void ConvertToCamelCase_ShouldWork()
    {
        var result = Utilities.Variable.ConvertToCamelCase("snake_case");
        Assert.That(result.result, Is.EqualTo("snakeCase"));
        Assert.That(result.to, Is.EqualTo(RequestedVariableNameTypeEnum.CamelCase));
    }

    [Test]
    public void ConvertTo_Generic_ShouldWork()
    {
        var result = Utilities.Variable.ConvertTo("my-variable", RequestedVariableNameTypeEnum.ScreamingSnakeCase);
        Assert.That(result.result, Is.EqualTo("MY_VARIABLE"));
        Assert.That(result.to, Is.EqualTo(RequestedVariableNameTypeEnum.ScreamingSnakeCase));
        Assert.That(result.from, Is.EqualTo(ResultsVariableNameTypeEnum.KebabCase));
    }

    [Test]
    public void ConvertToTitleWords_ShouldWork()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Utilities.Variable.ConvertToTitleWords("this is a test").result, Is.EqualTo("This Is a Test"));
            Assert.That(Utilities.Variable.ConvertToTitleWords("this_is_a_test").result, Is.EqualTo("This Is a Test"));
            Assert.That(Utilities.Variable.ConvertToTitleWords("This Is A Valid Title of Variable Words").result, Is.EqualTo("This Is a Valid Title of Variable Words"));
        });
    }

    [Test]
    public void ConvertToSentenceWords_ShouldWork()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Utilities.Variable.ConvertToSentenceWords("this is a test").result, Is.EqualTo("This is a test"));
            Assert.That(Utilities.Variable.ConvertToSentenceWords("snake_case_variable").result, Is.EqualTo("Snake case variable"));
        });
    }

    [Test]
    public void ConvertTo_AllTypes_ShouldWork()
    {
        string source = "my_variable_name"; // SnakeCase
        Assert.Multiple(() =>
        {
            Assert.That(Utilities.Variable.ConvertToCamelCase(source).result, Is.EqualTo("myVariableName"));
            Assert.That(Utilities.Variable.ConvertToPascalCase(source).result, Is.EqualTo("MyVariableName"));
            Assert.That(Utilities.Variable.ConvertToSnakeCase(source).result, Is.EqualTo("my_variable_name"));
            Assert.That(Utilities.Variable.ConvertToScreamingSnakeCase(source).result, Is.EqualTo("MY_VARIABLE_NAME"));
            Assert.That(Utilities.Variable.ConvertToKebabCase(source).result, Is.EqualTo("my-variable-name"));
            Assert.That(Utilities.Variable.ConvertToTrainCase(source).result, Is.EqualTo("My-Variable-Name"));
            Assert.That(Utilities.Variable.ConvertToUnicase(source).result, Is.EqualTo("myvariablename"));
            Assert.That(Utilities.Variable.ConvertToTrollCase(source).result, Is.EqualTo("MYVARIABLENAME"));
        });
    }

    [Test]
    public void ConvertTo_ShouldOutputEnumCorrectly()
    {
        var result = Utilities.Variable.ConvertToTrainCase("camelCase");
        Assert.That(result.to, Is.EqualTo(RequestedVariableNameTypeEnum.TrainCase));
    }

    [Test]
    public void GetVariableName_FromTypeAndInstance_ShouldWork()
    {
        var instance = new MyTestClass();
        var genericInstance = new MyGenericClass<int>();

        Assert.Multiple(() =>
        {
            Assert.That(Utilities.Variable.GetVariableName(typeof(MyTestClass)), Is.EqualTo("myTestClass"));
            Assert.That(Utilities.Variable.GetVariableName(typeof(MyGenericClass<int>)), Is.EqualTo("myGenericClass"));
            Assert.That(instance.ToVariableName(), Is.EqualTo("myTestClass"));
            Assert.That(genericInstance.ToVariableName(), Is.EqualTo("myGenericClass"));
            Assert.That(typeof(MyTestClass).ToVariableName(RequestedVariableNameTypeEnum.SnakeCase), Is.EqualTo("my_test_class"));
        });
    }

    [Test]
    public void GetClassName_FromTypeAndInstance_ShouldWork()
    {
        var instance = new MyTestClass();
        Assert.Multiple(() =>
        {
            Assert.That(Utilities.Variable.GetClassName(typeof(MyTestClass)), Is.EqualTo("MyTestClass"));
            Assert.That(instance.ToClassName(), Is.EqualTo("MyTestClass"));
            Assert.That(typeof(MyGenericClass<string>).ToClassName(), Is.EqualTo("MyGenericClass"));
        });
    }

    [Test]
    public void MemberInfo_Extensions_ShouldWork()
    {
        var prop = typeof(MyTestClass).GetProperty(nameof(MyTestClass.MyProperty));
        Assert.Multiple(() =>
        {
            Assert.That(prop!.ToVariableName(), Is.EqualTo("myProperty"));
            Assert.That(prop!.ToVariableName(RequestedVariableNameTypeEnum.SnakeCase), Is.EqualTo("my_property"));
        });
    }
}
