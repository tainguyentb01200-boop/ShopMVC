/**
 * PhoneShop MVC - Cart JavaScript
 * Handles all AJAX cart operations
 */

$(document).ready(function() {
    
    // ===== ADD TO CART - Product List & Detail =====
    $(document).off('click', '.add-to-cart-btn').on('click', '.add-to-cart-btn', function (e) {
        e.preventDefault();

        const $btn = $(this);

        // Chặn nếu nút đang trong trạng thái loading (click liên tục)
        if ($btn.hasClass('loading')) return;

        const productId = $btn.data('product-id');
        const quantity = $('#quantity').val() || 1;

        // Disable button and show loading
        $btn.prop('disabled', true).addClass('loading');
        const originalText = $btn.html();
        $btn.html('<i class="fa fa-spinner fa-spin"></i> Đang thêm...');

        $.ajax({
            url: '/Cart/AddToCart',
            type: 'POST',
            data: {
                productId: productId,
                quantity: quantity
            },
            success: function (response) {
                if (response.success) {
                    // Update cart count in header
                    updateCartCount(response.cartCount);

                    // Show success notification
                    showNotification('success', 'Đã thêm vào giỏ hàng!');

                    // Reset button after 1 second
                    setTimeout(function () {
                        $btn.prop('disabled', false)
                            .removeClass('loading')
                            .html(originalText);
                    }, 1000);
                } else {
                    // Show error message
                    showNotification('error', response.message || 'Có lỗi xảy ra!');

                    // Reset button
                    $btn.prop('disabled', false)
                        .removeClass('loading')
                        .html(originalText);
                }
            },
            error: function (xhr, status, error) {
                console.error('Add to Cart Error:', error);

                // Check if unauthorized
                if (xhr.status === 401) {
                    showNotification('warning', 'Vui lòng đăng nhập để thêm vào giỏ hàng');
                    setTimeout(function () {
                        window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                    }, 1500);
                } else {
                    showNotification('error', 'Không thể thêm vào giỏ hàng. Vui lòng thử lại!');
                }

                // Reset button
                $btn.prop('disabled', false)
                    .removeClass('loading')
                    .html(originalText);
            }
        });
    });
    
    
    // ===== UPDATE CART QUANTITY - Increase/Decrease Buttons =====
    $(document).on('click', '.update-quantity-btn', function(e) {
        e.preventDefault();
        
        const $btn = $(this);
        const cartDetailId = $btn.data('cart-detail-id');
        const action = $btn.data('action'); // 'increase' or 'decrease'
        const $row = $btn.closest('tr');
        const $quantityInput = $row.find('.quantity-input');
        let currentQty = parseInt($quantityInput.val());
        
        // Calculate new quantity
        let newQty = action === 'increase' ? currentQty + 1 : currentQty - 1;
        
        // Prevent going below 1
        if (newQty < 1) {
            showNotification('warning', 'Số lượng tối thiểu là 1');
            return;
        }
        
        // Check max stock
        const maxStock = parseInt($quantityInput.attr('max')) || 999;
        if (newQty > maxStock) {
            showNotification('warning', `Chỉ còn ${maxStock} sản phẩm trong kho`);
            return;
        }
        
        updateCartQuantity(cartDetailId, newQty, $row);
    });
    
    
    // ===== UPDATE CART QUANTITY - Direct Input Change =====
    $(document).on('change', '.quantity-input', function() {
        const $input = $(this);
        const cartDetailId = $input.data('cart-detail-id');
        const $row = $input.closest('tr');
        let newQty = parseInt($input.val());
        
        // Validate quantity
        if (isNaN(newQty) || newQty < 1) {
            newQty = 1;
            $input.val(1);
        }
        
        // Check max stock
        const maxStock = parseInt($input.attr('max')) || 999;
        if (newQty > maxStock) {
            showNotification('warning', `Chỉ còn ${maxStock} sản phẩm trong kho`);
            newQty = maxStock;
            $input.val(maxStock);
        }
        
        updateCartQuantity(cartDetailId, newQty, $row);
    });
    
    
    // ===== REMOVE FROM CART =====
    $(document).on('click', '.remove-cart-item-btn', function(e) {
        e.preventDefault();
        
        if (!confirm('Bạn có chắc muốn xóa sản phẩm này khỏi giỏ hàng?')) {
            return;
        }
        
        const $btn = $(this);
        const cartDetailId = $btn.data('cart-detail-id');
        const $row = $btn.closest('tr');
        
        // Show loading on button
        $btn.prop('disabled', true)
            .html('<i class="fa fa-spinner fa-spin"></i>');
        
        $.ajax({
            url: '/Cart/RemoveFromCart',
            type: 'POST',
            data: { cartDetailId: cartDetailId },
            success: function(response) {
                if (response.success) {
                    // Remove row with animation
                    $row.fadeOut(300, function() {
                        $(this).remove();
                        
                        // Update cart totals
                        updateCartTotals(response);
                        
                        // Check if cart is empty
                        if ($('.cart-item-row').length === 0) {
                            showNotification('info', 'Giỏ hàng trống');
                            setTimeout(function() {
                                location.reload();
                            }, 1000);
                        }
                    });
                    
                    showNotification('success', 'Đã xóa sản phẩm khỏi giỏ hàng');
                } else {
                    showNotification('error', response.message || 'Có lỗi xảy ra!');
                    $btn.prop('disabled', false)
                        .html('<i class="fa fa-trash"></i>');
                }
            },
            error: function() {
                showNotification('error', 'Không thể xóa sản phẩm. Vui lòng thử lại!');
                $btn.prop('disabled', false)
                    .html('<i class="fa fa-trash"></i>');
            }
        });
    });
    
    
    // ===== CLEAR CART =====
    $(document).on('click', '.clear-cart-btn', function(e) {
        e.preventDefault();
        
        if (!confirm('Bạn có chắc muốn xóa toàn bộ giỏ hàng?')) {
            return;
        }
        
        const $btn = $(this);
        $btn.prop('disabled', true)
            .html('<i class="fa fa-spinner fa-spin"></i> Đang xóa...');
        
        $.ajax({
            url: '/Cart/ClearCart',
            type: 'POST',
            success: function(response) {
                if (response.success) {
                    showNotification('success', 'Đã xóa toàn bộ giỏ hàng');
                    setTimeout(function() {
                        location.reload();
                    }, 1000);
                } else {
                    showNotification('error', response.message || 'Có lỗi xảy ra!');
                    $btn.prop('disabled', false)
                        .html('<i class="fa fa-trash-alt"></i> Xóa giỏ hàng');
                }
            },
            error: function() {
                showNotification('error', 'Không thể xóa giỏ hàng. Vui lòng thử lại!');
                $btn.prop('disabled', false)
                    .html('<i class="fa fa-trash-alt"></i> Xóa giỏ hàng');
            }
        });
    });
    
    
    // ===== QUANTITY SELECTOR (Product Detail Page) =====
    $('.quantity-decrease').on('click', function(e) {
        e.preventDefault();
        const $input = $('#quantity');
        let value = parseInt($input.val());
        if (value > 1) {
            $input.val(value - 1);
        }
    });
    
    $('.quantity-increase').on('click', function(e) {
        e.preventDefault();
        const $input = $('#quantity');
        const max = parseInt($input.attr('max')) || 999;
        let value = parseInt($input.val());
        if (value < max) {
            $input.val(value + 1);
        } else {
            showNotification('warning', `Chỉ còn ${max} sản phẩm trong kho`);
        }
    });
    
    // Validate quantity input
    $('#quantity').on('change', function() {
        const $input = $(this);
        let value = parseInt($input.val());
        const min = parseInt($input.attr('min')) || 1;
        const max = parseInt($input.attr('max')) || 999;
        
        if (isNaN(value) || value < min) {
            $input.val(min);
        } else if (value > max) {
            $input.val(max);
            showNotification('warning', `Chỉ còn ${max} sản phẩm trong kho`);
        }
    });
    
    
    // ===== HELPER FUNCTIONS =====
    
    /**
     * Update cart quantity via AJAX
     */
    function updateCartQuantity(cartDetailId, newQty, $row) {
        // Show loading state
        $row.addClass('updating');
        
        $.ajax({
            url: '/Cart/UpdateQuantity',
            type: 'POST',
            data: { 
                cartDetailId: cartDetailId, 
                quantity: newQty 
            },
            success: function(response) {
                if (response.success) {
                    // Update quantity input
                    $row.find('.quantity-input').val(newQty);
                    
                    // Update item subtotal
                    if (response.itemSubtotal !== undefined) {
                        $row.find('.item-subtotal').text(formatCurrency(response.itemSubtotal));
                    }
                    
                    // Update cart totals
                    updateCartTotals(response);
                    
                    showNotification('success', 'Đã cập nhật số lượng');
                } else {
                    showNotification('error', response.message || 'Có lỗi xảy ra!');
                    // Revert to original value
                    $row.find('.quantity-input').val($row.find('.quantity-input').data('original-value') || 1);
                }
                
                $row.removeClass('updating');
            },
            error: function() {
                showNotification('error', 'Không thể cập nhật số lượng. Vui lòng thử lại!');
                $row.find('.quantity-input').val($row.find('.quantity-input').data('original-value') || 1);
                $row.removeClass('updating');
            }
        });
    }
    
    /**
     * Update cart totals in the UI
     */
    function updateCartTotals(response) {
        // Update cart count in header
        if (response.cartCount !== undefined) {
            updateCartCount(response.cartCount);
        }
        
        // Update total amount
        if (response.totalAmount !== undefined) {
            $('.cart-total-amount').text(formatCurrency(response.totalAmount));
        }
        
        // Update subtotal
        if (response.subtotal !== undefined) {
            $('.cart-subtotal').text(formatCurrency(response.subtotal));
        }
        
        // Update shipping (if exists)
        if (response.shipping !== undefined) {
            $('.cart-shipping').text(formatCurrency(response.shipping));
        }
        
        // Update discount (if exists)
        if (response.discount !== undefined) {
            $('.cart-discount').text(formatCurrency(response.discount));
        }
    }


    /**
     * Update cart count in header
     */
    function updateCartCount(count) {
        $('.cart-count').text(count);
        
        // Update cart badge
        const $badge = $('.cart-badge');
        if (count > 0) {
            $badge.text(count).show();
        } else {
            $badge.hide();
        }
        
        // Animate cart icon
        $('.cart-icon').addClass('bounce');
        setTimeout(function() {
            $('.cart-icon').removeClass('bounce');
        }, 500);
    }
    
    /**
     * Format number as Vietnamese currency
     */
    function formatCurrency(amount) {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount);
    }
    
    /**
     * Show notification message
     */
    function showNotification(type, message) {
        // Remove existing notifications
        $('.cart-notification').remove();
        
        // Determine alert class
        const alertClass = type === 'success' ? 'alert-success' : 
                          type === 'error' ? 'alert-danger' : 
                          type === 'warning' ? 'alert-warning' :
                          'alert-info';
        
        // Determine icon
        const icon = type === 'success' ? 'fa-check-circle' : 
                    type === 'error' ? 'fa-exclamation-circle' : 
                    type === 'warning' ? 'fa-exclamation-triangle' :
                    'fa-info-circle';
        
        // Create notification element
        const $notification = $(`
            <div class="cart-notification alert ${alertClass} alert-dismissible fade show" 
                 style="position: fixed; top: 20px; right: 20px; z-index: 9999; min-width: 300px; box-shadow: 0 4px 12px rgba(0,0,0,0.15);" 
                 role="alert">
                <i class="fa ${icon} me-2"></i>
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        `);
        
        // Append to body
        $('body').append($notification);
        
        // Auto dismiss after 3 seconds
        setTimeout(function() {
            $notification.fadeOut(300, function() {
                $(this).remove();
            });
        }, 3000);
    }
    
    /**
     * Check if user is logged in (optional)
     */
    function isUserLoggedIn() {
        // This depends on your implementation
        // You can check session, cookie, or make an AJAX call
        return true; // Modify based on your auth system
    }
    
    
    // ===== MINI CART (Hover to show dropdown) =====
    $('.mini-cart-trigger').hover(
        function() {
            $(this).find('.mini-cart-dropdown').stop().fadeIn(200);
        },
        function() {
            $(this).find('.mini-cart-dropdown').stop().fadeOut(200);
        }
    );
    
    
    // ===== CART PAGE - Calculate totals on load =====
    if ($('.cart-page').length > 0) {
        calculateCartTotals();
    }
    
    function calculateCartTotals() {
        let subtotal = 0;
        
        $('.cart-item-row').each(function() {
            const $row = $(this);
            const price = parseFloat($row.find('.item-price').data('price')) || 0;
            const quantity = parseInt($row.find('.quantity-input').val()) || 0;
            const itemTotal = price * quantity;
            
            subtotal += itemTotal;
            
            // Update item subtotal
            $row.find('.item-subtotal').text(formatCurrency(itemTotal));
        });
        
        // Update subtotal
        $('.cart-subtotal').text(formatCurrency(subtotal));
        
        // Calculate shipping (example: free if > 500k, else 30k)
        const shipping = subtotal > 500000 ? 0 : 30000;
        $('.cart-shipping').text(formatCurrency(shipping));
        
        // Calculate total
        const total = subtotal + shipping;
        $('.cart-total-amount').text(formatCurrency(total));
    }
    
    
    // ===== ADD BOUNCE ANIMATION CSS =====
    if (!$('#cart-animations').length) {
        $('head').append(`
            <style id="cart-animations">
                .cart-icon.bounce {
                    animation: bounce 0.5s ease;
                }
                @keyframes bounce {
                    0%, 100% { transform: scale(1); }
                    50% { transform: scale(1.2); }
                }
                .updating {
                    opacity: 0.6;
                    pointer-events: none;
                }
            </style>
        `);
    }
    
});


// ===== STANDALONE FUNCTIONS (can be called from other scripts) =====

/**
 * Quick add to cart (can be called from anywhere)
 */
window.quickAddToCart = function(productId, quantity = 1) {
    return $.ajax({
        url: '/Cart/AddToCart',
        type: 'POST',
        data: { productId, quantity }
    });
};

/**
 * Get cart count
 */
window.getCartCount = function() {
    return parseInt($('.cart-count').text()) || 0;
};

/**
 * Refresh cart display
 */
window.refreshCart = function() {
    location.reload();
};
