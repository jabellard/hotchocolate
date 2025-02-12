using System;
using System.Text;
using Xunit;

namespace HotChocolate.Types.Relay;

public class IdSerializerTests
{
    [Fact]
    public void Serialize_TypeNameIsEmpty_ArgumentException()
    {
        // arrange
        var serializer = new IdSerializer();

        // act
        void Action() => serializer.Serialize("", 123);

        // assert
        Assert.Throws<ArgumentNullException>(Action);
    }

    [Fact]
    public void Serialize_IdIsNull_Null()
    {
        // arrange
        var serializer = new IdSerializer();

        // act
        var s = serializer.Serialize("Foo", default(object));

        // assert
        Assert.Null(s);
    }

    [Fact]
    public void Deserialize_SerializedIsNull_ArgumentNullException()
    {
        // arrange
        var serializer = new IdSerializer();

        // act
        void Action() => serializer.Deserialize(null);

        // assert
        Assert.Throws<ArgumentNullException>(Action);
    }

    [Fact]
    public void IsPossibleBase64String_sIsNull_ArgumentNullException()
    {
        // arrange
        // act
        void Action() => IdSerializer.IsPossibleBase64String(null!);

        // assert
        Assert.Throws<ArgumentNullException>(Action);
    }

    [InlineData("123", "\0Bar\nFoo\nd123")]
    [Theory]
    public void SerializeIdValueWithSchemaName(object id, string expected)
    {
        // arrange
        var schema = "Bar";
        var typeName = "Foo";
        var serializer = new IdSerializer(includeSchemaName: true);

        // act
        var serializedId = serializer.Serialize(schema, typeName, id);

        // assert
        var unwrapped = Encoding.UTF8.GetString(Convert.FromBase64String(serializedId!));
        Assert.Equal(expected, unwrapped);
    }

    [InlineData("AEJhcgpGb28KZDEyMw==", "123", typeof(string))]
    [Theory]
    public void DeserializeIdValueWithSchemaName(
        string serialized, object id, Type idType)
    {
        // arrange
        var serializer = new IdSerializer();

        // act
        var value = serializer.Deserialize(serialized);

        // assert
        Assert.IsType(idType, value.Value);
        Assert.Equal(id, value.Value);
        Assert.Equal("Foo", value.TypeName);
        Assert.Equal("Bar", value.SchemaName);
    }


    [InlineData((short)123, "Foo\ns123")]
    [InlineData(123, "Foo\ni123")]
    [InlineData((long)123, "Foo\nl123")]
    [InlineData("123456", "Foo\nd123456")]
    [Theory]
    public void SerializeIdValue(object id, string expected)
    {
        // arrange
        var typeName = "Foo";
        var serializer = new IdSerializer();

        // act
        var serializedId = serializer.Serialize(typeName, id);

        // assert
        var unwrapped = Encoding.UTF8.GetString(
            Convert.FromBase64String(serializedId!));
        Assert.Equal(expected, unwrapped);
    }

    [Fact]
    public void SerializeMaxLongIdValue()
    {
        // arrange
        object id = long.MaxValue;
        var expected = "Foo\nl" + id;
        var typeName = "Foo";
        var serializer = new IdSerializer();

        // act
        var serializedId = serializer.Serialize(typeName, id);

        // assert
        var unwrapped = Encoding.UTF8.GetString(
            Convert.FromBase64String(serializedId!));
        Assert.Equal(expected, unwrapped);
    }


    [Fact]
    public void SerializeGuidValue()
    {
        // arrange
        var typeName = "Foo";
        var id = new Guid("dad4f33d303345d7b7541d9ac23974d9");
        var serializer = new IdSerializer();

        // act
        var serializedId = serializer.Serialize(typeName, id);

        // assert
        var unwrapped = Encoding.UTF8.GetString(
            Convert.FromBase64String(serializedId!));
        Assert.Equal("Foo\ngdad4f33d303345d7b7541d9ac23974d9", unwrapped);
    }

    [InlineData("Rm9vCnMxMjM=", (short)123, typeof(short))]
    [InlineData("Rm9vCmkxMjM=", 123, typeof(int))]
    [InlineData("Rm9vCmwxMjM=", (long)123, typeof(long))]
    [InlineData("Rm9vCmQxMjM0NTY=", "123456", typeof(string))]
    [Theory]
    public void DeserializeIdValue(
        string serialized, object id, Type idType)
    {
        // arrange
        var serializer = new IdSerializer();

        // act
        var value = serializer.Deserialize(serialized);

        // assert
        Assert.IsType(idType, value.Value);
        Assert.Equal(id, value.Value);
        Assert.Equal("Foo", value.TypeName);
    }

    [Fact]
    public void DeserializeGuidValue()
    {
        // arrange
        var serialized = "Rm9vCmdkYWQ0ZjMzZDMwMzM0NWQ3Yjc1NDFkOWFjMjM5NzRkOQ==";
        var serializer = new IdSerializer();

        // act
        var value = serializer.Deserialize(serialized);

        // assert
        Assert.Equal(
            new Guid("dad4f33d303345d7b7541d9ac23974d9"),
            Assert.IsType<Guid>(value.Value));
        Assert.Equal("Foo", value.TypeName);
    }

    [InlineData("Rm9vLXN7===", false)]
    [InlineData("Rm9vLXN7====", false)]
    [InlineData("====", false)]
    [InlineData("=Rm9=vLXN7AA", false)]
    [InlineData("Rm9=vLXN7AA=", false)]
    [InlineData("Rm9vLXN7AA==", true)]
    [InlineData("Rm9vLWc989TaMzDXRbdUHZrCOXT", false)]
    [InlineData("Rm9vLWc989TaMzDXRbdUHZrCOXTZ", true)]
    [Theory]
    public void IsPossibleBase64String(string serialized, bool valid)
    {
        // arrange
        // act
        var result = IdSerializer.IsPossibleBase64String(serialized);

        // assert
        Assert.Equal(valid, result);
    }
}
