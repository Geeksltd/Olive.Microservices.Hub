({
    baseUrl: "../../", // comes from launchsettings
    paths: {
        // JQuery:
        "jquery": "../lib/jquery/dist/jquery",
        "jquery-ui/ui/widget": "../lib/jquery-ui/ui/widget",
        //"jquery-ui/ui/focusable": "jquery-ui/ui/focusable",
        "jquery-ui-all": "../lib/jquery-ui/jquery-ui",
        "jquery-validate": "../lib/jquery-validation/dist/jquery.validate",
        "jquery-validate-unobtrusive": "../lib/jquery-validation-unobtrusive/src/jquery.validate.unobtrusive",

        // Jquery plugins:
        "underscore": "../lib/underscore/underscore",
        "alertify": "../lib/alertifyjs/dist/js/alertify",
        "smartmenus": "../lib/smartmenus/src/jquery.smartmenus",
        "file-upload": "../lib/jquery-file-upload/js/jquery.fileupload",
        "jquery-typeahead": "../lib/jquery-typeahead/dist/jquery.typeahead.min",
        "combodate": "../lib/combodate/src/combodate",
        "js-cookie": "../lib/js-cookie/src/js.cookie",
        "handlebars": "../lib/handlebars/handlebars",
        "hammerjs": "../lib/hammer.js/hammer",
        "jquery-mentions": "../lib/jquery-mentions-input/jquery.mentionsInput",
        "chosen": "../lib/chosen-js/chosen.jquery",
        "jquery-elastic": "../lib/jquery-mentions-input/lib/jquery.elastic",
        "jquery-events-input": "../lib/jquery-mentions-input/lib/jquery.events.input",

        // Bootstrap
        "popper": "../lib/popper.js/dist/umd/popper",
        "bootstrap": "../lib/bootstrap/dist/js/bootstrap",
        "validation-style": "../lib/jquery-validation-bootstrap-tooltip/jquery-validate.bootstrap-tooltip",
        "file-style": "../lib/bootstrap-filestyle/src/bootstrap-filestyle",
        "spinedit": "../lib/bootstrap-spinedit/js/bootstrap-spinedit",
        "password-strength": "../lib/pwstrength-bootstrap/dist/pwstrength-bootstrap-1.2.7",
        "slider": "../lib/seiyria-bootstrap-slider/dist/bootstrap-slider.min",
        "moment": "../lib/moment/min/moment.min",
        "moment-locale": "../lib/moment/locale/en-gb",
        "datepicker": "../lib/eonasdan-bootstrap-datetimepicker/src/js/bootstrap-datetimepicker",
        "bootstrapToggle": "../lib/bootstrap-toggle/js/bootstrap-toggle",
        "bootstrap-select": "../lib/bootstrap-select/dist/js/bootstrap-select",
        "flickity": "../lib/flickity/dist/flickity.pkgd"
    },
    map: {
        "*": {
            "popper.js": "../lib/popper",
            '../moment': '../lib/moment',
            "jquery": "../lib/jquery/dist/jquery",
            "jquery-ui/ui/widget": "../lib/jquery-ui/ui/widget",
        "bootstrap": "../lib/bootstrap/dist/js/bootstrap",
        "bootstrap-select": "../lib/bootstrap-select/dist/js/bootstrap-select",
        "file-upload": "../lib/jquery-file-upload/js/jquery.fileupload",
        'olive': "../lib/olive.mvc/dist",
        "app": "../src/app",
            "jquery-validation": "../lib/jquery-validate",
            "jquery.validate.unobtrusive": "../lib/jquery-validate-unobtrusive",
            "jquery-sortable": "../lib/jquery-ui/ui/widgets/sortable"
        }
    },
    shim: {
        "underscore": {
            exports: '_'
        },
        "bootstrap": ["jquery", "popper"],
        "bootstrap-select": ['jquery', 'bootstrap'],
        "bootstrapToggle": ["jquery"],
        "jquery-validate": ['jquery'],
        "validation-style": ['jquery', "jquery-validate", "bootstrap"],
        "combodate": ['jquery'],
        "jquery-typeahead": ['jquery'],
        "file-upload": ['jquery', 'jquery-ui/ui/widget'],
        "file-style": ["file-upload"],
        "smartmenus": ['jquery'],
        "chosen": ['jquery'],
        "jquery-validate-unobtrusive": ['jquery-validate'],
        "spinedit": ['jquery'],
        "password-strength": ['jquery'],
        "moment-locale": ['moment'],
        "olive/extensions/jQueryExtensions": {
            deps: ['jquery', "jquery-validate-unobtrusive"]
        },
        "olive/olivePage": ["alertify", "olive/extensions/jQueryExtensions", "olive/extensions/systemExtensions", "combodate"],
        "appPage": ["jquery", "olive/olivePage"],
        "model/service": ["appPage", "olive/extensions/systemExtensions"],
        "featuresMenu/featuresMenu": ["model/service"],
        "featuresMenu/breadcrumbMenu": ["featuresMenu/featuresMenu"],
        "app": ["featuresMenu/breadcrumbMenu"],
        "jquery-elastic": ["jquery"],
        "jquery-events-input": ["jquery"],
        "jquery-mentions": ['jquery', "underscore", "jquery-elastic", "jquery-events-input"]
    },
    optimize: "none",
    //generateSourceMaps: false,
    //preserveLicenseComments: false,    
    name: "app/appPage",
    out: "../../../dist/bundle-built.js"
});