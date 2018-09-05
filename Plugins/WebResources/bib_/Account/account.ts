/// <reference path="../../node_modules/@types/xrm/index.d.ts" />


namespace webresources {

    export class Account {

        static getUrl() {

            var context = Xrm.Utility.getGlobalContext();

            alert("Greetings " + context.getUserName() + " Good Day?");
        }

        static setCreditLimit(formContext: Xrm.FormContext) {
         
            Xrm.Page.getAttribute("creditlimit").setValue(10000);
    }
}
}