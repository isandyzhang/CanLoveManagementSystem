/**
 * 防止重複提交的表單處理器
 * 使用方式：在表單上加入 class="prevent-double-submit"
 */
(function() {
    'use strict';
    
    // 儲存表單提交狀態
    const formStates = new WeakMap();
    
    // 等待 DOM 載入
    function init() {
        const forms = document.querySelectorAll('form.prevent-double-submit');
        
        forms.forEach(function(form) {
            // 初始化表單狀態
            formStates.set(form, { isSubmitting: false });
            
            form.addEventListener('submit', function(e) {
                const state = formStates.get(form);
                
                // 如果正在提交，阻止表單提交
                if (state.isSubmitting) {
                    e.preventDefault();
                    e.stopPropagation();
                    return false;
                }
                
                // 檢查表單驗證
                if (!form.checkValidity()) {
                    form.classList.add('was-validated');
                    e.preventDefault();
                    e.stopPropagation();
                    // 驗證失敗時不需要重置狀態，因為沒有開始提交
                    return false;
                }
                
                // 標記為正在提交
                state.isSubmitting = true;
                
                // 禁用所有提交按鈕
                const submitButtons = form.querySelectorAll('button[type="submit"], input[type="submit"]');
                submitButtons.forEach(function(btn) {
                    if (!btn.disabled) {
                        const originalText = btn.innerHTML;
                        btn.setAttribute('data-original-text', originalText);
                        btn.setAttribute('data-original-disabled', btn.disabled ? 'true' : 'false');
                        btn.disabled = true;
                        btn.innerHTML = '<i class="bi bi-arrow-repeat animate-spin me-1"></i>處理中...';
                    }
                });
                
                // 如果頁面重新載入（表單提交成功），狀態會自動重置
                // 如果表單驗證失敗，頁面不會重新載入，需要手動重置
                // 這裡使用 setTimeout 作為備用重置機制（5秒後自動重置，防止卡住）
                setTimeout(function() {
                    const currentState = formStates.get(form);
                    if (currentState && currentState.isSubmitting) {
                        resetFormSubmitState(form);
                    }
                }, 5000);
            });
        });
    }
    
    // 重置表單提交狀態
    function resetFormSubmitState(formElement) {
        if (!formElement) return;
        
        const state = formStates.get(formElement);
        if (state) {
            state.isSubmitting = false;
        }
        
        const submitButtons = formElement.querySelectorAll('button[type="submit"], input[type="submit"]');
        submitButtons.forEach(function(btn) {
            const originalText = btn.getAttribute('data-original-text');
            const originalDisabled = btn.getAttribute('data-original-disabled');
            
            if (originalText) {
                btn.innerHTML = originalText;
                btn.removeAttribute('data-original-text');
            }
            
            if (originalDisabled !== null) {
                btn.disabled = originalDisabled === 'true';
                btn.removeAttribute('data-original-disabled');
            }
        });
    }
    
    // DOM 載入後初始化
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
    
    // 提供全域函數來重置表單狀態（用於 AJAX 提交後或驗證失敗時）
    window.resetFormSubmitState = resetFormSubmitState;
})();

