document.addEventListener('DOMContentLoaded', () => {
    let cartButtons = document.querySelectorAll('[data-cart-product]');
    for (let btn of cartButtons) {
        btn.addEventListener('click', addCartClick);
    }

    for (let btn of document.querySelectorAll('[data-cart-detail-dec]')) {
        btn.addEventListener('click', decCartClick);
    }
    for (let btn of document.querySelectorAll('[data-cart-detail-inc]')) {
        btn.addEventListener('click', incCartClick);
    }
    for (let btn of document.querySelectorAll('[data-cart-detail-del]')) {
        btn.addEventListener('click', delCartClick);
    }
    for (let btn of document.querySelectorAll('[data-cart-detail-cnt]')) {
        btn.addEventListener('keydown', editCartEdit);
        btn.addEventListener('blur', editCartBlur);
        btn.addEventListener('focus', editCartFocus);
    }
    for (let btn of document.querySelectorAll('[data-cart-cancel]')) {
        btn.addEventListener('click', cancelCart);
    }
    for (let btn of document.querySelectorAll('[data-cart-buy]')) {
        btn.addEventListener('click', buyCart);
    }
});

function buyCart(e) {
    const idElement = e.target.closest("[data-cart-buy]");
    if (!idElement) throw "buyCart() error: [data-cart-buy] not found";

    const cartId = idElement.getAttribute("data-cart-buy");
    if (!cartId) throw "buyCart() error: [data-cart-buy] attribute empty or not found";

    console.log(cartId);
}
function cancelCart(e) {
    const idElement = e.target.closest("[data-cart-cancel]");
    if (!idElement) throw "cancelCart() error: [data-cart-cancel] not found";

    const cartId = idElement.getAttribute("data-cart-cancel");
    if (!cartId) throw "cancelCart() error: [data-cart-cancel] attribute empty or not found";

    console.log(cartId);

}
/*
Додати діалоги погодження операцій роботи з елементами кошику:
видалення - "Ви видаляєте позицію 'Кіт' з кошику. Підтверджуєте? " [так][ні]
ручне введення кількості - "Ви змінюєте кількість замовлення 'Кіт' з 2 до 10 шт. Підтверджуєте? " [так][ні]
** реалізувати діалогами Bootstrap
*/
function editCartFocus(e) {
    e.target.beforeEditing = e.target.innerText;
}
function editCartBlur(e) {
    if (e.target.innerText === "") e.target.innerText = e.target.beforeEditing;

    if (e.target.beforeEditing != e.target.innerText) {
        const delta = Number(e.target.innerText) - Number(e.target.beforeEditing);
        const cdElement = e.target.closest("[data-cart-detail-cnt]");
        const cdId = cdElement.getAttribute("data-cart-detail-cnt");

        console.log(`Changes: ${e.target.beforeEditing} -> ${e.target.innerText} d=${delta} id=${cdId}`);

        fetch(`/Shop/ModifyCart/${cdId}?delta=${delta}`, {
            method: 'PATCH'
        }).then(r => r.json())
            .then(j => {
                console.log(j);
                if (j.status < 300) {
                    window.location.reload();
                }
                else {
                    alert("Помилка: " + j.message);
                    e.target.innerText = e.target.beforeEditing;
                }
            });
    }    
}
function editCartEdit(e) {
    if (![8, 13, 37, 39, 46, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57].includes(e.keyCode)) {
        e.preventDefault();
        return true;
    }
    if (e.keyCode == 13) {
        e.target.blur();
    }
}

function delCartClick(e) {
    e.stopPropagation();
    const cdElement = e.target.closest("[data-cart-detail-del]");
    const cdId = cdElement.getAttribute("data-cart-detail-del");
    const spanElement = cdElement.parentNode.querySelector('[data-cart-detail-cnt]');
    const delta = -Number(spanElement.innerText);
    fetch(`/Shop/ModifyCart/${cdId}?delta=${delta}`, {
        method: 'PATCH'
    }).then(r => r.json())
        .then(j => {
            console.log(j);
            if (j.status < 300) {
                window.location.reload();
            }
            else {
                alert("Помилка: " + j.message);
            }
        });
}

function decCartClick(e) {
    e.stopPropagation();
    const cdElement = e.target.closest("[data-cart-detail-dec]");
    const cdId = cdElement.getAttribute("data-cart-detail-dec");
    console.log("- " + cdId);

    fetch(`/Shop/ModifyCart/${cdId}?delta=-1`, {
        method: 'PATCH'
    }).then(r => r.json())
        .then(j => {
            console.log(j);
            if (j.status < 300) {
                location = location;
            }
            else {
                alert("Помилка: " + j.message);
            }
        });
}

function incCartClick(e) {
    e.stopPropagation();
    const cdElement = e.target.closest("[data-cart-detail-inc]");
    const cdId = cdElement.getAttribute("data-cart-detail-inc");
    console.log("+ " + cdId);

    fetch(`/Shop/ModifyCart/${cdId}?delta=1`, {
        method: 'PATCH'
    }).then(r => r.json())
        .then(j => {
            console.log(j);
            if (j.status < 300) {
                window.location.reload();
            }
            else {
                alert("Помилка: " + j.message);
            }
        });

}

function addCartClick(e) {
    e.stopPropagation();
    const cartElement = e.target.closest("[data-cart-product]");
    const productId = cartElement.getAttribute("data-cart-product");
    console.log(productId);
    fetch("/Shop/AddToCart/" + productId, {
        method: 'PUT'
    }).then(r => r.json()).then(j => {
        console.log(j);
        if (j.status == 401) {
            alert("Увійдіть до системи для замовлення товарів");
        }
        else if (j.status == 201) {
            if (confirm("Товар додано до кошику. Перейти до кошику?")) {
                window.location = '/User/Cart';
            }
        }
        else {
            alert("Щось пішло шкереберть");
        }
    });
}


document.addEventListener('submit', e => {
    const form = e.target;
    if (form.id === "auth-form") {
        e.preventDefault();
        const login = form.querySelector('[name="UserLogin"]').value;
        const password = form.querySelector('[name="Password"]').value;
        if (login.length == 0) {
            alert("Введіть логін");
            return;
        }
        if (password.length == 0) {
            alert("Введіть пароль");
            return;
        }
        // RFC 7617
        const credentials = btoa(login + ':' + password);
        fetch("/User/Authenticate", {
            method: "GET",
            headers: {
                'Authorization': 'Basic ' + credentials
            }
        }).then(r => {
            console.log(r);
            if (r.ok) {
                window.location.reload();
            }
            else {
                r.json().then(j => {
                    alert(j);   // Д.З.
                });
            }
        });

        console.log(credentials);
    }
});
