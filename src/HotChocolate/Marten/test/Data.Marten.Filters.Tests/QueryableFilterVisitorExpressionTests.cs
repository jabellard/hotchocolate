using CookieCrumble;
using HotChocolate.Execution;

namespace HotChocolate.Data.Filters;

public class QueryableFilterVisitorExpressionTests : IClassFixture<SchemaCache>
{
    private static readonly Foo[] _fooEntities =
    {
        new()
        {
            Name = "Foo",
            LastName = "Galoo",
            Bars = new[]
            {
                new Bar { Value="A" }
            }
        },
        new()
        {
            Name = "Sam",
            LastName = "Sampleman",
            Bars = Array.Empty<Bar>()
        }
    };

    private readonly SchemaCache _cache;

    public QueryableFilterVisitorExpressionTests(SchemaCache cache)
    {
        _cache = cache;
    }

    [Fact]
    public async Task Create_StringConcatExpression()
    {
        // arrange
        var tester = _cache.CreateSchema<Foo, FooFilterInputType>(_fooEntities);

        // act
        var res1 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
            .SetQuery("{ root(where: { displayName: { eq: \"Sam Sampleman\"}}){ name lastName}}")
            .Create());

        var res2 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
            .SetQuery("{ root(where: { displayName: { eq: \"NoMatch\"}}){ name lastName}}")
            .Create());

        var res3 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
            .SetQuery("{ root(where: { displayName: { eq: null}}){ name lastName}}")
            .Create());

        // assert
        await SnapshotExtensions.AddResult(
                SnapshotExtensions.AddResult(
                    SnapshotExtensions.AddResult(
                        Snapshot
                            .Create(), res1, "Sam_Sampleman"), res2, "NoMatch"), res3, "null")
            .MatchAsync();
    }

    [Fact]
    public async Task Create_CollectionLengthExpression()
    {
        // arrange
        var tester = _cache.CreateSchema<Foo, FooFilterInputType>(_fooEntities);

        // act
        var res1 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
            .SetQuery("{ root(where: { barLength: { eq: 1}}){ name lastName}}")
            .Create());

        var res2 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
            .SetQuery("{ root(where: { barLength: { eq: 0}}){ name lastName}}")
            .Create());

        var res3 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
            .SetQuery("{ root(where: { barLength: { eq: null}}){ name lastName}}")
            .Create());

        // assert
        await SnapshotExtensions.AddResult(
                SnapshotExtensions.AddResult(
                    SnapshotExtensions.AddResult(
                        Snapshot
                            .Create(), res1, "1"), res2, "0"), res3, "null")
            .MatchAsync();
    }

    public class Foo
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? LastName { get; set; }

        public ICollection<Bar>? Bars { get; set; }
    }

    public class Bar
    {
        public int Id { get; set; }

        public string? Value { get; set; }
    }

    public class FooFilterInputType : FilterInputType<Foo>
    {
        protected override void Configure(IFilterInputTypeDescriptor<Foo> descriptor)
        {
            descriptor.Field(x => x.Name + " " + x.LastName).Name("displayName");
            descriptor.Field(x => x.Bars!.Count).Name("barLength");
        }
    }
}
