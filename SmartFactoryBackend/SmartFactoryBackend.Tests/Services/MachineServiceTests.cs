using Moq;
using SmartFactoryBackend.Application.Interfaces;
using SmartFactoryBackend.Application.Services;
using SmartFactoryBackend.Domain.Enums;
using SmartFactoryBackend.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFactoryBackend.Tests.Services
{
    public class MachineServiceTests
    {
        private readonly MachineService _machineService;
        private readonly Mock<IMachineRepository> _machineRepositoryMock;
        public MachineServiceTests() 
        {
            _machineRepositoryMock = new Mock<IMachineRepository>();
            _machineService = new MachineService(_machineRepositoryMock.Object);
        }

        [Fact]
        public async Task GetMachinesAsync_ReturnsAllMachines()
        {
            // Arrange
            var machines = new List<ChocolateMachine>
            {
                new() { Name = "Line #1" },
                new() { Name = "Line #2" }
            };

            _machineRepositoryMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(machines);

            // Act
            var result = await _machineService.GetMachinesAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, m => m.Name == "Line #1");
        }

        [Fact]
        public async Task GetHistoryAsync_MachineDoesNotExist_ReturnsNull()
        {
            // Arrange
            var machineId = Guid.NewGuid();

            _machineRepositoryMock
                .Setup(r => r.GetByIdAsync(machineId))
                .ReturnsAsync((ChocolateMachine?)null);

            // Act
            var result = await _machineService.GetHistoryAsync(machineId, 5);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetHistoryAsync_MachineExists_ReturnsMappedReadings()
        {
            // Arrange
            var machineId = Guid.NewGuid();
            var machine = new ChocolateMachine { Id = machineId, Name = "Line #1" };

            var readings = new List<TelemetryReading>
        {
            new() { Id = Guid.NewGuid(), MachineId = machineId, Temperature = 32.5, Status = MachineStatus.Normal, Timestamp = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), MachineId = machineId, Temperature = 38.0, Status = MachineStatus.Warning, Timestamp = DateTime.UtcNow }
        };

            _machineRepositoryMock
                .Setup(r => r.GetByIdAsync(machineId))
                .ReturnsAsync(machine);

            _machineRepositoryMock
                .Setup(r => r.GetHistoryAsync(machineId, 5))
                .ReturnsAsync(readings);

            // Act
            var result = await _machineService.GetHistoryAsync(machineId, 5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result!.Count);
            Assert.Equal(32.5, result[0].Temperature);
        }

        [Fact]
        public async Task AddMachineAsync_CallsRepositoryWithCorrectName()
        {
            // Act
            var result = await _machineService.AddMachineAsync("New Line");

            // Assert
            Assert.Equal("New Line", result.Name);
            _machineRepositoryMock.Verify(
                r => r.AddAsync(It.Is<ChocolateMachine>(m => m.Name == "New Line")),
                Times.Once);
        }

        [Fact]
        public async Task AddMachineAsync_NameAlreadyTaken_ThrowsInvalidOperationException()
        {
            _machineRepositoryMock.Setup(r=> r.ExistsByNameAsync("Line #1")).ReturnsAsync(true);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _machineService.AddMachineAsync("Line #1"));

            Assert.Equal("Name is already taken.", exception.Message);

            _machineRepositoryMock.Verify(r => r.AddAsync(It.IsAny<ChocolateMachine>()), Times.Never);
        }

        [Fact]
        public async Task DeleteMachineAsync_ExistingMachine_ReturnsTrue()
        {
            var machineId = Guid.NewGuid();

            _machineRepositoryMock.Setup(r => r.DeleteAsync(machineId)).ReturnsAsync(true);

            var result = await _machineService.DeleteMachineAsync(machineId);

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteMachineAsync_NonExistingMachine_ReturnsFalse()
        {
            var machineId = Guid.NewGuid();

            _machineRepositoryMock.Setup(r => r.DeleteAsync(machineId)).ReturnsAsync(false);

            var result = await _machineService.DeleteMachineAsync(machineId);

            Assert.False(result);
        }
    }
}
