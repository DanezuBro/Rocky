var dataTable;

$(document).ready(function () {
    loadDataTable("GetInquiryList");
});

function loadDataTable(url) {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Inquiry/" + url
        },
        "columns": [
            { "data": "id", "width": "10%" },
            { "data": "fullName", "width": "15%" },
            { "data": "phoneNumber", "width": "15%" },
            { "data": "email", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                            <div class="text-center">
                                <a href="/Inquiry/Details/${data}" class="btn btn-primary text-white" style="cursor:pointer">
                                    <i class="fa-solid fa-pen-to-square"></i> 
                                </a>
                            </div>
                           `;
                }, "width": "5%"
            }
        ]
    });
}
