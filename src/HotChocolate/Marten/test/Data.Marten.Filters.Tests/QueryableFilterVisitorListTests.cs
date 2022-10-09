using CookieCrumble;
using HotChocolate.Execution;

namespace HotChocolate.Data.Filters;

public class QueryableFilterVisitorListTests : IClassFixture<SchemaCache>
{
    private static readonly Foo[] _fooEntities =
    {
        new()
        {
            FooNested = new[]
            {
                new FooNested { Bar = "a" },
                new FooNested { Bar = "a" },
                new FooNested { Bar = "a" }
            }
        },
        new()
        {
            FooNested = new[]
            {
                new FooNested { Bar = "c" },
                new FooNested { Bar = "a" },
                new FooNested { Bar = "a" }
            }
        },
        new()
        {
            FooNested = new[]
            {
                new FooNested { Bar = "a" },
                new FooNested { Bar = "d" },
                new FooNested { Bar = "b" }
            }
        },
        new()
        {
            FooNested = new[]
            {
                new FooNested { Bar = "c" },
                new FooNested { Bar = "d" },
                new FooNested { Bar = "b" }
            }
        },
        new()
        {
            FooNested = new[]
            {
                new FooNested { Bar = null },
                new FooNested { Bar = "d" },
                new FooNested { Bar = "b" }
            }
        }
    };

    private readonly SchemaCache _cache;

    public QueryableFilterVisitorListTests(SchemaCache cache)
    {
        _cache = cache;
    }

    [Fact]
    public async Task Create_ArraySomeObjectStringEqualWithNull_Expression()
    {
        // arrange
        var tester = _cache.CreateSchema<Foo, FooFilterInput>(_fooEntities);

        // act
        var res1 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery(
                    @"{
                        root(where: {
                            fooNested: {
                                some: {
                                    bar: {
                                        eq: ""a""
                                    }
                                }
                            }
                        }){
                            fooNested {
                                bar
                            }
                        }
                    }")
                .Create());

        var res2 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery(
                    "{ root(where: { fooNested: { some: {bar: { eq: \"d\"}}}}){ fooNested {bar}}}")
                .Create());

        var res3 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery(
                    "{ root(where: { fooNested: { some: {bar: { eq: null}}}}){ fooNested {bar}}}")
                .Create());

        // assert
        await SnapshotExtensions.AddResult(
                SnapshotExtensions.AddResult(
                    SnapshotExtensions.AddResult(Snapshot.Create(), res1, "a"),
                    res2,
                    "d"),
                res3,
                "null")
            .MatchAsync();
    }

    [Fact]
    public async Task Create_ArrayNoneObjectStringEqual_Expression()
    {
        // arrange
        var tester = _cache.CreateSchema<Foo, FooFilterInput>(_fooEntities);

        // act
        var res1 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery(
                    "{ root(where: { fooNested: { none: {bar: { eq: \"a\"}}}}){ fooNested {bar}}}")
                .Create());

        var res2 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery(
                    "{ root(where: { fooNested: { none: {bar: { eq: \"d\"}}}}){ fooNested {bar}}}")
                .Create());

        var res3 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery(
                    "{ root(where: { fooNested: { none: {bar: { eq: null}}}}){ fooNested {bar}}}")
                .Create());

        // assert
        await SnapshotExtensions.AddResult(
                SnapshotExtensions.AddResult(
                    SnapshotExtensions.AddResult(Snapshot.Create(),
                        res1,
                        "a"),
                    res2,
                    "d"),
                res3,
                "null")
            .MatchAsync();
    }

    [Fact]
    public async Task Create_ArrayAllObjectStringEqual_Expression()
    {
        // arrange
        var tester = _cache.CreateSchema<Foo, FooFilterInput>(_fooEntities);

        // act
        var res1 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery(
                    "{ root(where: { fooNested: { all: {bar: { eq: \"a\"}}}}){ fooNested {bar}}}")
                .Create());

        var res2 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery(
                    "{ root(where: { fooNested: { all: {bar: { eq: \"d\"}}}}){ fooNested {bar}}}")
                .Create());

        var res3 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery(
                    "{ root(where: { fooNested: { all: {bar: { eq: null}}}}){ fooNested {bar}}}")
                .Create());

        // assert
        await SnapshotExtensions.AddResult(
                SnapshotExtensions.AddResult(
                    SnapshotExtensions.AddResult(
                        Snapshot
                            .Create(),
                        res1,
                        "a"),
                    res2,
                    "d"),
                res3,
                "null")
            .MatchAsync();
    }

    [Fact]
    public async Task Create_ArrayAllObjectStringEqual_Expression_CustomAllow()
    {
        // arrange
        var tester = _cache.CreateSchema<Foo, FooCustomAllowsFilterInput>(_fooEntities);

        // act
        var res1 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery(
                    "{ root(where: { fooNested: { all: {bar: { eq: \"a\"}}}}){ fooNested {bar}}}")
                .Create());

        var res2 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery(
                    "{ root(where: { fooNested: { all: {bar: { eq: \"d\"}}}}){ fooNested {bar}}}")
                .Create());

        var res3 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery(
                    "{ root(where: { fooNested: { all: {bar: { eq: null}}}}){ fooNested {bar}}}")
                .Create());

        // assert
        await SnapshotExtensions.AddResult(
                SnapshotExtensions.AddResult(
                    SnapshotExtensions.AddResult(
                        Snapshot
                            .Create(),
                        res1,
                        "a"),
                    res2,
                    "d"),
                res3,
                "null")
            .MatchAsync();
    }

    [Fact]
    public async Task Create_ArrayAnyObjectStringEqual_Expression()
    {
        // arrange
        var tester = _cache.CreateSchema<Foo, FooFilterInput>(_fooEntities);

        // act
        var res1 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery(
                    "{ root(where: { fooNested: { any: false}}){ fooNested {bar}}}")
                .Create());

        var res2 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery(
                    "{ root(where: { fooNested: { any: true}}){ fooNested {bar}}}")
                .Create());

        var res3 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery(
                    "{ root(where: { fooNested: { all: null}}){ fooNested {bar}}}")
                .Create());

        // assert
        await SnapshotExtensions.AddResult(
                SnapshotExtensions.AddResult(
                    SnapshotExtensions.AddResult(
                        Snapshot
                            .Create(),
                        res1,
                        "false"),
                    res2,
                    "true"),
                res3,
                "null")
            .MatchAsync();
    }

    public class Foo
    {
        public int Id { get; set; }

        public IEnumerable<FooNested?>? FooNested { get; set; }
    }

    public class FooSimple
    {
        public IEnumerable<string?>? Bar { get; set; }
    }

    public class FooNested
    {
        public int Id { get; set; }

        public string? Bar { get; set; }
    }

    public class FooFilterInput : FilterInputType<Foo>
    {
        protected override void Configure(IFilterInputTypeDescriptor<Foo> descriptor)
        {
            descriptor.Field(t => t.FooNested);
        }
    }

    public class FooCustomAllowsFilterInput : FilterInputType<Foo>
    {
        protected override void Configure(IFilterInputTypeDescriptor<Foo> descriptor)
        {
            descriptor.Field(
                f => f.FooNested,
                o =>
                {
                    o.AllowAll(f => f.Field(y => y.Bar).AllowEquals());
                    o.AllowSome(f => f.Field(y => y.Bar).AllowEquals());
                    o.AllowNone(f => f.Field(y => y.Bar).AllowEquals());
                });
        }
    }

    public class FooSimpleFilterInput : FilterInputType<FooSimple>
    {
        protected override void Configure(IFilterInputTypeDescriptor<FooSimple> descriptor)
        {
            descriptor.Field(t => t.Bar);
        }
    }
}