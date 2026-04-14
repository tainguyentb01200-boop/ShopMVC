// ============================================
// PHONESHOP - CART.JS
// AJAX Cart Operations
// ============================================

$(document).ready(function () {

    // ==========================================
    // ADD TO CART
    // ==========================================
    $(document).on('click', '.add-to-cart-btn', function (e) {
        e.preventDefault();

        const $btn = $(this);
        const productId = $btn.data('product-id');
        const quantity = $('.cart-input').val() || 1;

        // Disable button
        $btn.prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Đang thêm...');

        $.ajax({
            url: '/Cart/AddToCart',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                productId: parseInt(productId),
                quantity: parseInt(quantity)
            }),
            success: function (response) {
                if (response.success) {
                    // Update cart count in header
                    $('.cart-count').text(response.cartCount);

                    // Show success message
                    showNotification('success', response.message || 'Đã thêm vào giỏ hàng!');

                    // Reset button
                    $btn.prop('disabled', false).html('<i class="fa fa-shopping-cart"></i> Thêm vào giỏ');
                } else {
                    if (response.requireLogin) {
                        // Redirect to login
                        showNotification('warning', 'Vui lòng đăng nhập để tiếp tục');
                        setTimeout(function () {
                            window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                        }, 1500);
                    } else {
                        showNotification('error', response.message || 'Không thể thêm vào giỏ hàng');
                        $btn.prop('disabled', false).html('<i class="fa fa-shopping-cart"></i> Thêm vào giỏ');
                    }
                }
            },
            error: function (xhr, status, error) {
                console.error('Add to cart error:', error);
                showNotification('error', 'Có lỗi xảy ra, vui lòng thử lại');
                $btn.prop('disabled', false).html('<i class="fa fa-shopping-cart"></i> Thêm vào giỏ');
            }
        });
    });

    // ==========================================
    // UPDATE CART QUANTITY
    // ==========================================
    $(document).on('click', '.update-cart-btn', function (e) {
        e.preventDefault();

        const $btn = $(this);
        const productId = $btn.data('product-id');
        const $quantityInput = $btn.closest('.cart-item').find('.cart-input');
        const quantity = $quantityInput.val();

        if (quantity <= 0) {
            showNotification('warning', 'Số lượng phải lớn hơn 0');
            return;
        }

        $btn.prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i>');

        $.ajax({
            url: '/Cart/UpdateQuantity',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                productId: parseInt(productId),
                quantity: parseInt(quantity)
            }),
            success: function (response) {
                if (response.success) {
                    showNotification('success', 'Đã cập nhật số lượng');
                    // Reload page to update totals
                    location.reload();
                } else {
                    showNotification('error', response.message || 'Không thể cập nhật');
                    $btn.prop('disabled', false).html('Cập nhật');
                }
            },
            error: function () {
                showNotification('error', 'Có lỗi xảy ra');
                $btn.prop('disabled', false).html('Cập nhật');
            }
        });
    });

    // ==========================================
    // REMOVE FROM CART
    // ==========================================
    $(document).on('click', '.remove-cart-btn', function (e) {
        e.preventDefault();

        if (!confirm('Bạn có chắc muốn xóa sản phẩm này?')) {
            return;
        }

        const $btn = $(this);
        const productId = $btn.data('product-id');

        $btn.prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i>');

        $.ajax({
            url: '/Cart/Remove',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                productId: parseInt(productId)
            }),
            success: function (response) {
                if (response.success) {
                    showNotification('success', 'Đã xóa sản phẩm');
                    // Reload page
                    location.reload();
                } else {
                    showNotification('error', response.message || 'Không thể xóa sản phẩm');
                    $btn.prop('disabled', false).html('<i class="fa fa-times"></i>');
                }
            },
            error: function () {
                showNotification('error', 'Có lỗi xảy ra');
                $btn.prop('disabled', false).html('<i class="fa fa-times"></i>');
            }
        });
    });

    // ==========================================
    // QUANTITY PLUS/MINUS BUTTONS
    // ==========================================
    $(document).on('click', '.qty-btn.plus', function (e) {
        e.preventDefault();
        const $input = $(this).siblings('.cart-input');
        const max = parseInt($input.attr('max')) || 999;
        let value = parseInt($input.val()) || 1;
        if (value < max) {
            $input.val(value + 1);
        }
    });

    $(document).on('click', '.qty-btn.minus', function (e) {
        e.preventDefault();
        const $input = $(this).siblings('.cart-input');
        let value = parseInt($input.val()) || 1;
        if (value > 1) {
            $input.val(value - 1);
        }
    });

    // ==========================================
    // NOTIFICATION HELPER
    // ==========================================
    function showNotification(type, message) {
        // Using Bootstrap toast or simple alert
        // You can replace this with a better notification library
        
        const bgClass = {
            'success': 'alert-success',
            'error': 'alert-danger',
            'warning': 'alert-warning',
            'info': 'alert-info'
        }[type] || 'alert-info';

        const $notification = $(`
            <div class="alert ${bgClass} alert-dismissible fade show position-fixed" 
                 role="alert" 
                 style="top: 20px; right: 20px; z-index: 9999; min-width: 300px;">
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `);

        $('body').append($notification);

        // Auto dismiss after 3 seconds
        setTimeout(function () {
            $notification.fadeOut(function () {
                $(this).remove();
            });
        }, 3000);
    }
});
