using CookieCrumble;
using HotChocolate.Data.Filters;
using HotChocolate.Execution;
using NetTopologySuite.Geometries;
using Squadron;

namespace HotChocolate.Data.Spatial.Filters;

[Collection("Postgres")]
public class QueryableFilterVisitorOverlapsTests : SchemaCache
{
    private static readonly Polygon _truePolygon =
        new(new LinearRing(new[]
        {
            new Coordinate(150, 150),
            new Coordinate(270, 150),
            new Coordinate(190, 70),
            new Coordinate(140, 20),
            new Coordinate(20, 20),
            new Coordinate(70, 70),
            new Coordinate(150, 150)
        }));

    private static readonly Polygon _falsePolygon =
        new(new LinearRing(new[]
        {
            new Coordinate(1000, 1000),
            new Coordinate(100000, 1000),
            new Coordinate(100000, 100000),
            new Coordinate(1000, 100000),
            new Coordinate(1000, 1000),
        }));

    private static readonly Foo[] _fooEntities =
    {
        new() { Id = 1, Bar = _truePolygon },
        new() { Id = 2, Bar = _falsePolygon }
    };

    public QueryableFilterVisitorOverlapsTests(PostgreSqlResource<PostgisConfig> resource)
        : base(resource)
    {
    }

    [Fact]
    public async Task Create_Overlaps_Query()
    {
        // arrange
        var tester = await CreateSchemaAsync<Foo, FooFilterType>(_fooEntities);

        // act
        var res1 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery(
                    @"{
                        root(where: {
                            bar: {
                                overlaps: {
                                    geometry: {
                                        type: Polygon,
                                        coordinates: [
                                            [
                                                [150 150],
                                                [270 150],
                                                [330 150],
                                                [250 70],
                                                [190 70],
                                                [70 70],
                                                [150 150]
                                            ]
                                        ]
                                    }
                                }
                            }
                        }){
                            id
                        }
                    }")
                .Create());

        var res2 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery(
                    @"{
                        root(where: {
                            bar: {
                                noverlaps: {
                                    geometry: {
                                        type: Polygon,
                                        coordinates: [
                                            [
                                                [150 150],
                                                [270 150],
                                                [330 150],
                                                [250 70],
                                                [190 70],
                                                [70 70],
                                                [150 150]
                                            ]
                                        ]
                                    }
                                }
                            }
                        }){
                            id
                        }
                    }")
                .Create());

        // assert
        await SnapshotExtensions.AddResult(
                SnapshotExtensions.AddResult(
                    Snapshot
                        .Create(), res1, "true"), res2, "false")
            .MatchAsync();
    }

    public class Foo
    {
        public int Id { get; set; }

        public Polygon Bar { get; set; } = null!;
    }

    public class FooFilterType : FilterInputType<Foo>
    {
    }
}
