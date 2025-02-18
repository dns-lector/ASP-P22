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
    }
});

function editCartEdit(e) {
    console.log(e); return;  // keyCode 48-57 8 46 37 39
    

    e.stopPropagation();
    const cdElement = e.target.closest("[data-cart-detail-cnt]");
    const cdId = cdElement.getAttribute("data-cart-detail-cnt");
    console.log("edit " + cdId);
}

function delCartClick(e) {
    e.stopPropagation();
    const cdElement = e.target.closest("[data-cart-detail-del]");
    const cdId = cdElement.getAttribute("data-cart-detail-del");
    console.log("x " + cdId);
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
