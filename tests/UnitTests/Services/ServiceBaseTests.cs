using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ProjectTemplate.Application.Services;
using ProjectTemplate.Domain.Exceptions;
using ProjectTemplate.Domain.Interfaces;

namespace ProjectTemplate.UnitTests.Services;

public class ServiceBaseTests
{
    public class TestEntity { public long Id { get; set; } }

    public class ConcreteService(
        IRepository<TestEntity> repository,
        ILogger<Service<TestEntity>> logger)
        : Service<TestEntity>(repository, logger);

    private readonly Mock<IRepository<TestEntity>> _mockRepo = new();
    private readonly Mock<ILogger<Service<TestEntity>>> _mockLogger = new();
    private readonly ConcreteService _service;

    public ServiceBaseTests()
    {
        _service = new ConcreteService(_mockRepo.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByIdAsync_DelegatesToRepository()
    {
        var entity = new TestEntity { Id = 1 };
        _mockRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        var result = await _service.GetByIdAsync(1);

        result.Should().BeSameAs(entity);
    }

    [Fact]
    public async Task GetAllAsync_DelegatesToRepository()
    {
        var entities = new List<TestEntity> { new() { Id = 1 }, new() { Id = 2 } };
        _mockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(entities);

        var result = await _service.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateAsync_CallsAddThenSaveChanges()
    {
        var entity = new TestEntity { Id = 1 };
        _mockRepo.Setup(r => r.AddAsync(entity, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        await _service.CreateAsync(entity);

        _mockRepo.Verify(r => r.AddAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ReturnsEntityFromRepository()
    {
        var entity = new TestEntity { Id = 1 };
        var stored = new TestEntity { Id = 42 };
        _mockRepo.Setup(r => r.AddAsync(entity, It.IsAny<CancellationToken>())).ReturnsAsync(stored);

        var result = await _service.CreateAsync(entity);

        result.Should().BeSameAs(stored);
    }

    [Fact]
    public async Task UpdateAsync_CallsUpdateThenSaveChanges()
    {
        var entity = new TestEntity { Id = 1 };

        await _service.UpdateAsync(1, entity);

        _mockRepo.Verify(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingEntity_CallsDeleteThenSaveChanges()
    {
        var entity = new TestEntity { Id = 1 };
        _mockRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        await _service.DeleteAsync(1);

        _mockRepo.Verify(r => r.DeleteAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingEntity_ThrowsNotFoundException()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((TestEntity?)null);

        var act = () => _service.DeleteAsync(99);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*99*");
    }

    [Fact]
    public async Task GetPagedAsync_DelegatesToRepository()
    {
        var items = new List<TestEntity> { new() { Id = 1 } };
        _mockRepo.Setup(r => r.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((items.AsEnumerable(), 1));

        var (result, total) = await _service.GetPagedAsync(1, 10);

        result.Should().HaveCount(1);
        total.Should().Be(1);
    }
}
