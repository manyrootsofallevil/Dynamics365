/// <reference path="../../node_modules/@types/xrm/index.d.ts" />
var webresources;
(function (webresources) {
    var Account = /** @class */ (function () {
        function Account() {
        }
        Account.getUrl = function () {
            var context = Xrm.Utility.getGlobalContext();
            alert("Greetings " + context.getUserName() + " Good Day?");
        };
        Account.setCreditLimit = function (formContext) {
            Xrm.Page.getAttribute("creditlimit").setValue(10000);
        };
        return Account;
    }());
    webresources.Account = Account;
})(webresources || (webresources = {}));
//# sourceMappingURL=account.js.map