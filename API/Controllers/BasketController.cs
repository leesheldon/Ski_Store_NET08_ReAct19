using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class BasketController(StoreContext context, 
    DiscountService discountService, PaymentsService paymentsService) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<BasketDto>> GetBasket()
    {
        var basket = await RetrieveBasket();
        
        if (basket == null) return NoContent();

        return basket.ToDto();
    }

    [HttpPost]
    public async Task<ActionResult<BasketDto>> AddItemToBasket(int productId, int quantity)
    {
        var basket = await RetrieveBasket();

        // ??= means: if basket is null, then execute the create basket function.
        basket ??= CreateBasket();

        var product = await context.Products.FindAsync(productId);
        if (product == null) return BadRequest("Cannot find product to add item to basket.");

        basket.AddItem(product, quantity);

        var result = await context.SaveChangesAsync() > 0;
        // the result is True, if the changes > 0

        if (result) return CreatedAtAction(nameof(GetBasket), basket.ToDto());

        return BadRequest("Problem in updating adding item to basket.");
    }

    [HttpDelete]
    public async Task<ActionResult> RemoveBasketItem(int productId, int quantity)
    {
        var basket = await RetrieveBasket();

        if (basket == null) return BadRequest("Cannot find basket to remove item.");

        basket.RemoveItem(productId, quantity);
        
        var result = await context.SaveChangesAsync() > 0;

        if (result) return Ok();

        return BadRequest("Problem in updating removing basket item.");
    }

    private async Task<Basket?> RetrieveBasket()
    {
        return await context.Baskets
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.BasketId == Request.Cookies["basketId"]);
    }

    private Basket CreateBasket()
    {
        var basketId = Guid.NewGuid().ToString();
        var cookieOptions = new CookieOptions
        {
            IsEssential = true,
            Expires = DateTime.UtcNow.AddDays(30)
        };

        Response.Cookies.Append("basketId", basketId, cookieOptions);

        var basket = new Basket { BasketId = basketId};
        context.Baskets.Add(basket);

        return basket;
    }

    [HttpPost("{code}")]
    public async Task<ActionResult<BasketDto>> AddCouponCode(string code)
    {
        //Get the basket
        var basket = await RetrieveBasket();
        if (basket == null || string.IsNullOrEmpty(basket.ClientSecret))
            return BadRequest("Unable to apply voucher as basket not found.");

        // Get the coupon
        var coupon = await discountService.GetCouponFromPromoCode(code);
        if (coupon == null) return BadRequest("Invalid coupon.");

        // Update the basket with the coupon
        basket.Coupon = coupon;

        // Update the payment intent
        var intent = await paymentsService.CreateOrUpdatePaymentIntent(basket);
        if (intent == null) return BadRequest("Problem in applying coupon to basket.");

        // Save changes and return BasketDto if successful
        var result = await context.SaveChangesAsync() > 0;

        if (result) return CreatedAtAction(nameof(GetBasket), basket.ToDto());

        return BadRequest("Problem in updating basket.");
    }

    [HttpDelete("remove-coupon")]
    public async Task<ActionResult> RemoveCouponFromBasket()
    {
        // Get the basket
        var basket = await RetrieveBasket();
        if (basket == null || basket.Coupon == null || string.IsNullOrEmpty(basket.ClientSecret)) 
            return BadRequest("Unable to update basket with coupon as basket not found.");
        
        var intent = await paymentsService.CreateOrUpdatePaymentIntent(basket, true);
        if (intent == null) return BadRequest("Problem in removing coupon from basket.");

        basket.Coupon = null;

        var result = await context.SaveChangesAsync() > 0;
        
        if (result) return Ok();

        return BadRequest("Problem in updating basket.");
    }
}
