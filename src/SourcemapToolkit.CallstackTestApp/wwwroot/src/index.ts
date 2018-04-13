
const els = {
    get callstackdisplay() {
        return document.getElementById("callstackdisplay") as HTMLPreElement;
    },
    get deminified() {
        return document.getElementById("deminified") as HTMLPreElement;
    },
    get crashbutton() {
        return document.getElementById("crashbutton") as HTMLButtonElement;
    }
}

function causeCrash() {
    function level1() {
        var longLocalVariableName = 16;
        longLocalVariableName += 2;
        level2(longLocalVariableName);
    }

    function level2(input: number) {
        input = input + 2;
        level3(input);
    }

    function level3(input: number) {
        (function () {
            let x: any;
            console.log(x.length + input);
        }());
    }

    window.onerror = function (message, source, lineno, colno, error) {
        let stack;
        if (error) {
            stack = error.stack;
        } else if (window.event && (window.event as any).error) {
            stack = (window.event as any).error.stack;
        }
        els.callstackdisplay.innerText = stack;
        deminify(stack);
    }

    level1();
}

function deminify(stack: string) {
    els.deminified.innerText = "";
    fetch(`/Deminify/Deminify?stack=${encodeURIComponent(stack)}`, { method: "GET" })
        .then(res => res.text())
        .then(txt => {
            els.deminified.innerText = txt;
        });
}

window.onload = function (event) {
    els.crashbutton.addEventListener("click", () => {
        causeCrash();
    });
}