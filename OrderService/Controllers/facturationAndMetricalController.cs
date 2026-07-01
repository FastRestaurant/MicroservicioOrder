using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.UseCases;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using Application.UseCases.Facturation.Commands;
using Application.UseCases.Facturation.Queries;


using OrderService.Domain.Exceptions;
using OrderService.Presentation.Authorization;
using System.Security.Claims;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/v1/orders")]
   // [Authorize]
    public class FacturationAndMetricalController : ControllerBase
    {
        private readonly IGetFacturasHandler _getFacturasHandler;
        private readonly IConfirmPaymentHandler _confirmPaymentHandler;

        public FacturationAndMetricalController(
            IGetFacturasHandler getFacturasHandler,
            IConfirmPaymentHandler confirmPaymentHandler)
        {
            _getFacturasHandler = getFacturasHandler;
            _confirmPaymentHandler = confirmPaymentHandler;
        }


        [HttpGet("facturas")]
        public async Task<IActionResult> GetFacturas([FromQuery] GetFacturasQuery query)
        {
            var result = await _getFacturasHandler.Handle(query);
            return Ok(result);
        }


        [HttpPut("facturas/{id}/pay")]
        public async Task<IActionResult> ConfirmPayment(int id)
        {
            var command = new ConfirmPaymentCommand { FacturaId = id };

            var result = await _confirmPaymentHandler.Handle(command);

            if (!result)
                return NotFound();

            return NoContent();
        }

    }
}
