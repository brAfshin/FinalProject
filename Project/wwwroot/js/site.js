var SinglePage = {};

SinglePage.LoadModal = function () {
    var url = window.location.hash.toLowerCase();
    if (url.startsWith("#showmodal")) {
        url = url.split("showmodal=")[1];
        $.get(url,
            null,
            function (htmlPage) {
                $("#ModalContent").html(htmlPage);
                const container = document.getElementById("ModalContent");
                const forms = container.getElementsByTagName("form");
                const newForm = forms[forms.length - 1];
                $.validator.unobtrusive.parse(newForm);
                showModal();
            }).fail(function (error) {
                alert("An error has been occured.");
            });
    }
};

function showModal() {
    $("#MainModal").modal("show");
}

function hideModal() {
    $("#MainModal").modal("hide");

}

$(document).ready(function () {
    window.onhashchange = function () {
        SinglePage.LoadModal();
    };
    $("#MainModal").on("shown.bs.modal",
        function () {
            window.location.hash = "##";
            $('.persianDateInput').persianDatepicker({
                format: 'YYYY/MM/DD',
                autoClose: true,
                initialValueType: "persian"
            });
            $("#ddlCustomers").select2();
            $("body").on("change", "#ddlCustomers", function () {
                $("input[name=CustomerName]").val($(this).find("option:selected").text());
            });
        }
    );

    $(document).on("submit",
        'form[data-ajax="true"]',
        function (e) {
            e.preventDefault();
            var form = $(this);
            const method = form.attr("method").toLocaleLowerCase();
            const url = form.attr("action");
            var action = form.attr("data-action");

            if (method === "get") {
                const data = form.serializeArray();
                $.get(url,
                    data,
                    function (data) {
                        CallBackHandler(data, action, form);
                    });
            } else {
                var formData = new FormData(this);
                $.ajax({
                    url: url,
                    type: "post",
                    data: formData,
                    enctype: "multipart/form-data",
                    dataType: "json",
                    processData: false,
                    contentType: false,
                    success: function (data) {
                        CallBackHandler(data, action, form);
                    },
                    error: function (data) {
                        alert("An error has been occured.");
                    }

                });
            }
            return false;
        });

    $(document).on("submit",
        'form[confirm-dialog="true"]',
        function (e) {
            e.preventDefault();
            var formRaw = this;
            var form = $(this);
            const method = form.attr("method").toLocaleLowerCase();
            const url = form.attr("action");
            var action = form.attr("data-action");
            alertify.okBtn("Yes")
                .cancelBtn("No").confirm('<p style="text-align: right !important">Are you sure about this operation?</p>', function (ev) {
                    var formData = new FormData(formRaw);
                    $.ajax({
                        url: url,
                        type: "post",
                        data: formData,
                        enctype: "multipart/form-data",
                        dataType: "json",
                        processData: false,
                        contentType: false,
                        success: function (data) {
                            CallBackHandler(data, action, form);
                        },
                        error: function (data) {
                            alert("An error has been occured.");
                        }

                    });

                    return false;

                }, function (ev) {
                    ev.preventDefault();
                });
        });
});

$(document).ready(function () {
    $('#datatable').DataTable({
        "aaSorting": [],
        responsive: true
    });

    //Buttons examples
    var table = $('#datatable-buttons').DataTable({
        "aaSorting": [],
        buttons: ['copy', 'excel', 'print', 'pdf', 'colvis']
    });

    table.buttons().container()
        .appendTo('#datatable-buttons_wrapper .col-md-6:eq(0)');
});