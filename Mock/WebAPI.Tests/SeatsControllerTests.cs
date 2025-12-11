using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers;
using WebAPI.Exceptions;
using WebAPI.Models;
using WebAPI.Services;

namespace WebAPI.Tests;

[TestClass]
public class SeatsControllerTests
{
    // Mock of the SeatsService (fake version of the real service)
    // We control what this returns or throws to test the controller behavior.
    Mock<SeatsService> serviceMock;

    // Mock of the SeatsController.
    // We use a *partial mock* (CallBase = true), meaning:
    // - All real controller methods run normally
    // - Only specific things we "setup" are mocked.
    Mock<SeatsController> controllerMock;

    public SeatsControllerTests()
    {
        // Create the mock SeatsService
        serviceMock = new Mock<SeatsService>();

        // Create a mock controller that uses the mocked service
        controllerMock = new Mock<SeatsController>(serviceMock.Object) { CallBase = true };

        // Fake the UserId property so the controller always thinks
        // the user ID is "11111" during the tests.
        controllerMock.Setup(c => c.UserId).Returns("11111");
    }

    // -------------------------------------------------------------
    // TEST 1 — When the service returns a valid seat
    // The controller should return HTTP 200 OK with that seat.
    // -------------------------------------------------------------
    [TestMethod]
    public void ReserveSeat()
    {
        // Create a fake seat object that the service will return
        Seat seat = new Seat();
        seat.Id = 1;
        seat.Number = 1;

        // Setup the service mock so that:
        // Whenever ReserveSeat is called with ANY string + ANY int,
        // it will return this seat object.
        serviceMock.Setup(s => s.ReserveSeat(It.IsAny<string>(), It.IsAny<int>()))
                   .Returns(seat);

        // Call the controller method we want to test
        var actionresult = controllerMock.Object.ReserveSeat(seat.Number);

        // Since the service succeeded, the controller should return OkObjectResult (HTTP 200)
        var result = actionresult.Result as OkObjectResult;

        // Check that the result is indeed OK
        Assert.IsNotNull(result);
    }

    // -------------------------------------------------------------
    // TEST 2 — Service throws SeatAlreadyTakenException
    // Controller should return HTTP 401 Unauthorized
    // -------------------------------------------------------------
    [TestMethod]
    public void ReserveSeat_SeatAlreadyTaken()
    {
        // Fake the service to always throw SeatAlreadyTakenException
        serviceMock.Setup(s => s.ReserveSeat(It.IsAny<string>(), It.IsAny<int>()))
                   .Throws(new SeatAlreadyTakenException());

        // Call the controller
        var actionresult = controllerMock.Object.ReserveSeat(1);

        // Since seat is taken, the controller should return UnauthorizedResult (HTTP 401)
        var result = actionresult.Result as UnauthorizedResult;

        Assert.IsNotNull(result);
    }

    // -------------------------------------------------------------
    // TEST 3 — Service throws SeatOutOfBoundsException
    // Controller should return HTTP 404 NotFound with message
    // "Could not find {seatNumber}"
    // -------------------------------------------------------------
    [TestMethod]
    public void ReserveSeat_SeatOutOfBounds()
    {
        // Fake the service to throw SeatOutOfBoundsException
        serviceMock.Setup(s => s.ReserveSeat(It.IsAny<string>(), It.IsAny<int>()))
                   .Throws(new SeatOutOfBoundsException());

        var seatNumber = 1;

        // Call the controller
        var actionresult = controllerMock.Object.ReserveSeat(seatNumber);

        // Controller should return NotFoundObjectResult (HTTP 404)
        var result = actionresult.Result as NotFoundObjectResult;

        Assert.IsNotNull(result);

        // Verify the message returned inside the NotFound
        // IMPORTANT: This ensures correct error message formatting
        Assert.AreEqual("Could not find " + seatNumber, result.Value);
    }

    // -------------------------------------------------------------
    // TEST 4 — Service throws UserAlreadySeatedException
    // Controller should return HTTP 400 BadRequest
    // -------------------------------------------------------------
    [TestMethod]
    public void ReserveSeat_UserAlreadySeated()
    {
        // Fake the service to throw
        serviceMock.Setup(s => s.ReserveSeat(It.IsAny<string>(), It.IsAny<int>()))
                   .Throws(new UserAlreadySeatedException());

        // Call the controller
        var actionresult = controllerMock.Object.ReserveSeat(1);

        // BadRequestResult = HTTP 400
        var result = actionresult.Result as BadRequestResult;

        Assert.IsNotNull(result);
    }
}
