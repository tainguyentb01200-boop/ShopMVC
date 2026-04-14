/**
 * PhoneShop MVC - Site.js
 * Custom JavaScript for the application
 */

$(document).ready(function() {
    console.log('PhoneShop MVC - Site.js loaded successfully');
    
    // ===== Smooth Scroll =====
    $('a[href^="#"]').on('click', function(e) {
        var target = $(this.hash);
        if (target.length) {
            e.preventDefault();
            $('html, body').animate({
                scrollTop: target.offset().top - 80
            }, 800);
        }
    });
    
    // ===== Back to Top Button =====
    if ($('.back-to-top').length) {
        $(window).scroll(function() {
            if ($(this).scrollTop() > 300) {
                $('.back-to-top').fadeIn();
            } else {
                $('.back-to-top').fadeOut();
            }
        });
        
        $('.back-to-top').click(function(e) {
            e.preventDefault();
            $('html, body').animate({scrollTop: 0}, 800);
        });
    }
    
    // ===== Image Lazy Loading Fallback =====
    $('img').each(function() {
        $(this).on('error', function() {
            // If image fails to load, replace with placeholder
            var width = $(this).width() || 300;
            var height = $(this).height() || 300;
            $(this).attr('src', `https://via.placeholder.com/${width}x${height}/667eea/ffffff?text=No+Image`);
        });
    });
    
    // ===== Form Validation Enhancement =====
    $('form').on('submit', function() {
        var $form = $(this);
        var $submitBtn = $form.find('button[type="submit"]');
        
        // Disable submit button to prevent double submission
        $submitBtn.prop('disabled', true);
        
        // Re-enable after 3 seconds (in case of client-side validation failure)
        setTimeout(function() {
            $submitBtn.prop('disabled', false);
        }, 3000);
    });
    
    // ===== Tooltip Initialization (Bootstrap 5) =====
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
    
    // ===== Popover Initialization (Bootstrap 5) =====
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
    
    // ===== Alert Auto Dismiss =====
    $('.alert:not(.alert-permanent)').each(function() {
        var $alert = $(this);
        setTimeout(function() {
            $alert.fadeOut(300, function() {
                $(this).remove();
            });
        }, 5000); // Auto dismiss after 5 seconds
    });
    
    // ===== Mobile Menu Toggle =====
    $('.mobile-menu-toggle').on('click', function() {
        $('body').toggleClass('mobile-menu-active');
        $(this).toggleClass('active');
    });
    
    // Close mobile menu when clicking outside
    $(document).on('click', function(e) {
        if (!$(e.target).closest('.mobile-menu, .mobile-menu-toggle').length) {
            $('body').removeClass('mobile-menu-active');
            $('.mobile-menu-toggle').removeClass('active');
        }
    });
    
    // ===== Search Box Enhancement =====
    $('.search-toggle').on('click', function(e) {
        e.preventDefault();
        $('.search-box').toggleClass('active');
        $('.search-box input').focus();
    });
    
    // ===== Quantity Input Validation =====
    $('input[type="number"]').on('input', function() {
        var $input = $(this);
        var min = parseInt($input.attr('min')) || 1;
        var max = parseInt($input.attr('max')) || 999;
        var value = parseInt($input.val());
        
        if (isNaN(value) || value < min) {
            $input.val(min);
        } else if (value > max) {
            $input.val(max);
        }
    });
    
    // ===== Product Image Gallery (if exists) =====
    $('.product-thumbnail').on('click', function(e) {
        e.preventDefault();
        var $thumb = $(this);
        var newSrc = $thumb.attr('href') || $thumb.find('img').attr('src');
        
        $('.product-main-image img').attr('src', newSrc);
        $('.product-thumbnail').removeClass('active');
        $thumb.addClass('active');
    });
    
    // ===== Price Range Slider (if exists) =====
    if ($('#price-range').length && typeof $.fn.slider === 'function') {
        $('#price-range').slider({
            range: true,
            min: 0,
            max: 50000000,
            values: [0, 50000000],
            slide: function(event, ui) {
                $('#price-min').text(formatCurrency(ui.values[0]));
                $('#price-max').text(formatCurrency(ui.values[1]));
            }
        });
    }
    
    // ===== Helper Functions =====
    
    function formatCurrency(amount) {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND',
            minimumFractionDigits: 0
        }).format(amount);
    }
    
    // ===== Console Welcome Message =====
    console.log('%c🛒 PhoneShop MVC', 'color: #667eea; font-size: 24px; font-weight: bold;');
    console.log('%cWelcome to PhoneShop! Built with ASP.NET Core MVC', 'color: #666; font-size: 14px;');
});


// ===== Global Functions =====

/**
 * Show a toast notification
 */
window.showToast = function(type, message, duration = 3000) {
    const alertClass = type === 'success' ? 'alert-success' : 
                      type === 'error' ? 'alert-danger' : 
                      type === 'warning' ? 'alert-warning' :
                      'alert-info';
    
    const icon = type === 'success' ? 'fa-check-circle' : 
                type === 'error' ? 'fa-exclamation-circle' : 
                type === 'warning' ? 'fa-exclamation-triangle' :
                'fa-info-circle';
    
    const $toast = $(`
        <div class="toast-notification alert ${alertClass} alert-dismissible fade show" 
             style="position: fixed; top: 20px; right: 20px; z-index: 9999; min-width: 300px; box-shadow: 0 4px 12px rgba(0,0,0,0.15);" 
             role="alert">
            <i class="fa ${icon} me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `);
    
    $('body').append($toast);
    
    setTimeout(function() {
        $toast.fadeOut(300, function() {
            $(this).remove();
        });
    }, duration);
};

/**
 * Confirm dialog
 */
window.confirmDialog = function(message, callback) {
    if (confirm(message)) {
        if (typeof callback === 'function') {
            callback();
        }
        return true;
    }
    return false;
};

/**
 * Format number as Vietnamese currency
 */
window.formatVND = function(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND',
        minimumFractionDigits: 0
    }).format(amount);
};
