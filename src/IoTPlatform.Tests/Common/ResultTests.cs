using FluentAssertions;
using IoTPlatform.Common.Results;
using Xunit;

namespace IoTPlatform.Tests.Common;

public class ResultTests
{
    [Fact]
    public void Success_HasNoError()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_CarriesError()
    {
        var error = Error.NotFound("device missing");
        var result = Result.Failure(error);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void GenericSuccess_ExposesValue()
    {
        Result<int> result = 42;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void GenericFailure_ThrowsOnValueAccess()
    {
        Result<int> result = Error.Validation("bad input");

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }
}
