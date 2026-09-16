var empr_StichingOrder = {
    InitEvents: function () {
        $(document).ready(function () {
            empr_StichingOrder.InitSuitType();
            empr_StichingOrder.ResetForm();

            if ($("#CONTACT_NO").mask) {
                $("#CONTACT_NO").mask("9999-9999999");
            }

            $('body').on('click', '#BtnQuickSearch', function () {
                empr_StichingOrder.InitQuickSearchGrid();
            });

            $('body').on('click', '.elm_edit', function () {
                var reportid = $(this).attr("reportid");
                empr_StichingOrder.GetOrderById(reportid);
            });

            $('body').on('click', '#BtnNew', function () {
                $('#BtnDelete').hide();
                empr_StichingOrder.ResetForm();
            });

            $('body').on('click', '#BtnEnter', function () {
                empr_StichingOrder.EnterCustomer();
            });

            $('body').on('click', '#BtnEnterContact', function () {
                empr_StichingOrder.EnterCustomerByContact();
            });

            $('body').on('keypress', '#ID', function (e) {
                if (e.which === 13) {
                    e.preventDefault();
                    $('#BtnEnter').click();
                }
            });

            $('body').on('keypress', '#CONTACT_NO', function (e) {
                if (e.which === 13) {
                    e.preventDefault();
                    $('#BtnEnterContact').click();
                }
            });

            $('body').on('click', '#BtnSave', function () {
                if (Permissions != "Admin") {
                    if (!$("#ORDER_ID").val() && !Permissions.r_ADD) {
                        empr_helper.notify("You are not allowed to add new record !", 2);
                    }
                    else if (($("#ORDER_ID").val() > 0) && !Permissions.r_EDIT) {
                        empr_helper.notify("You are not allowed to edit records !", 2);
                    } else {
                        if (empr_StichingOrder.ValidateForm()) {
                            empr_StichingOrder.Save();
                        }
                    }
                } else {
                    if (empr_StichingOrder.ValidateForm()) {
                        empr_StichingOrder.Save();
                    }
                }
            });

            $('body').on('click', '#BtnDelete', function () {
                empr_StichingOrder.Delete();
            });

            if (Permissions != "Admin") {
                !Permissions.r_ADD && $('#BtnNew').hide();
                !Permissions.r_VIEW && $('#BtnQuickSearch').hide();
                (!Permissions.r_ADD && !Permissions.r_EDIT) && $('#BtnSave').hide();
            }
        });
    },
    ResetForm: function () {
        $("#ORDER_ID").val('');
        $("#ID").val('');
        $("#FULL_NAME").val('');
        $("#CONTACT_NO").val('');
        empr_StichingOrder.ClearOrderFields();
        $('#BtnDelete').hide();
        if (Permissions != "Admin") {
            if (Permissions.r_ADD) {
                $('#BtnSave').show();
            } else {
                $('#BtnSave').hide();
            }
        } else {
            $('#BtnSave').show();
        }
    },
    ClearOrderFields: function () {
        $("#ORDER_ID").val('');
        empr_StichingOrder.SetSuitTypeValue(null);
        $("#KURTA_LENGTH").val('');
        $("#SHOULDER").val('');
        $("#SLEEVES").val('');
        $("#CHEST").val('');
        $("#WAIST").val('');
        $("#COLLAR_SIZE").val('');
        $("#ARMHOLE").val('');
        $("#CUFF_MORI").val('');
        $("#BOTTOM_LENGTH").val('');
        $("#PANCHA").val('');
        $("#ASAN_GHERA").val('');
        $('input[name="BOTTOM_TYPE"]').prop('checked', false);
        $('input[name="DAMAN_STYLE"]').prop('checked', false);
        $('input[name="GALA_STYLE"]').prop('checked', false);
        $('input[name="PATTI_STYLE"]').prop('checked', false);
        $('input[name="FRONT_POCKET"]').prop('checked', false);
        $('input[name="SIDE_POCKETS"]').prop('checked', false);
        $('input[name="FITTING_STYLE"]').prop('checked', false);
    },
    ValidateForm: function () {
        var valid = true;
        var FULL_NAME = ($("#FULL_NAME").val() || '').trim();
        if (FULL_NAME == '') {
            valid = false;
            empr_helper.notify("Please enter customer name.", 2);
        }
        return valid;
    },
    GetDataToSave: function () {
        var idVal = $("#ID").val();
        var orderIdVal = $("#ORDER_ID").val();
        var suitTypeVal = $('#SUIT_TYPE').dxSelectBox('instance').option('value');
        var customerId = (idVal === undefined || idVal === null || idVal === '') ? 0 : idVal;
        var modelRecord = {
            ID: customerId,
            CUSTOMER_ID: customerId,
            ORDER_ID: (orderIdVal === undefined || orderIdVal === null || orderIdVal === '') ? 0 : orderIdVal,
            FULL_NAME: ($("#FULL_NAME").val() || '').trim(),
            CONTACT_NO: ($("#CONTACT_NO").val() || '').trim(),
            SUIT_TYPE: (suitTypeVal === undefined || suitTypeVal === null || suitTypeVal === '') ? '' : String(suitTypeVal),
            KURTA_LENGTH: $("#KURTA_LENGTH").val(),
            SHOULDER: $("#SHOULDER").val(),
            SLEEVES: $("#SLEEVES").val(),
            CHEST: $("#CHEST").val(),
            WAIST: $("#WAIST").val(),
            COLLAR_SIZE: $("#COLLAR_SIZE").val(),
            ARMHOLE: $("#ARMHOLE").val(),
            CUFF_MORI: $("#CUFF_MORI").val(),
            BOTTOM_TYPE: $('input[name="BOTTOM_TYPE"]:checked').val() || '',
            BOTTOM_LENGTH: $("#BOTTOM_LENGTH").val(),
            PANCHA: $("#PANCHA").val(),
            ASAN_GHERA: $("#ASAN_GHERA").val(),
            DAMAN_STYLE: $('input[name="DAMAN_STYLE"]:checked').val() || '',
            GALA_STYLE: $('input[name="GALA_STYLE"]:checked').val() || '',
            PATTI_STYLE: $('input[name="PATTI_STYLE"]:checked').val() || '',
            FRONT_POCKET: $('input[name="FRONT_POCKET"]:checked').val() || '',
            SIDE_POCKETS: $('input[name="SIDE_POCKETS"]:checked').val() || '',
            FITTING_STYLE: $('input[name="FITTING_STYLE"]:checked').val() || ''
        };
        return modelRecord;
    },
    Save: function () {
        var obj = empr_StichingOrder.GetDataToSave();
        ajaxHelper.ajaxPostJsonData(obj, "/StichingOrder/Save", function (data) {
            empr_helper.notify(data.msg, data.msgType);
            if (data.msgType == 1) {
                empr_StichingOrder.ResetForm();
                $('#BtnDelete').hide();
            }
        }, false, true);
    },
    Delete: function () {
        swal({
            title: 'Are you sure you want to remove this record?',
            text: "You won't be able to revert this!",
            type: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#0CC27E',
            cancelButtonColor: '#FF586B',
            confirmButtonText: 'Yes, delete it!',
            cancelButtonText: 'No, cancel!',
            confirmButtonClass: 'btn btn-success mr-5',
            cancelButtonClass: 'btn btn-danger',
            buttonsStyling: false
        }).then(function () {
            ajaxHelper.ajaxPostJsonData({ id: $('#ORDER_ID').val() }, "/StichingOrder/Delete", function (data) {
                empr_helper.notify(data.msg, data.msgType);
                if (data.msgType == 1) {
                    empr_StichingOrder.ResetForm();
                    $('#BtnDelete').hide();
                }
            }, false, true);
        });
    },
    InitQuickSearchGrid: function () {
        ajaxHelper.ajaxGetJson('/StichingOrder/QuickSearch', function (data) {
            if (data.msgType == 1) {
                empr_StichingOrder.CreateQuickSearchGrid(data.data);
            }
            else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },
    EnterCustomer: function () {
        var idVal = ($("#ID").val() || '').trim();
        if (idVal === '') {
            empr_helper.notify("Please enter serial no.", 2);
            return;
        }
        empr_StichingOrder.GetCustomerById(idVal);
    },
    EnterCustomerByContact: function () {
        var contactNo = ($("#CONTACT_NO").val() || '').trim();
        if (contactNo === '' || contactNo.indexOf('_') > -1) {
            empr_helper.notify("Please enter contact number.", 2);
            return;
        }
        empr_StichingOrder.GetCustomerByContactNo(contactNo);
    },
    BindCustomerLookup: function (record) {
        $("#ORDER_ID").val('');
        $("#ID").val(record.id || '');
        $("#FULL_NAME").val(record.fulL_NAME || '');
        $("#CONTACT_NO").val(record.contacT_NO || '');
        empr_StichingOrder.FillMeasurementFields(record);
        $('#BtnDelete').hide();
        if (Permissions != "Admin") {
            if (Permissions.r_ADD) {
                $('#BtnSave').show();
            } else {
                $('#BtnSave').hide();
            }
            if (Permissions.r_ADD) {
                $('#BtnNew').show();
            }
        } else {
            $('#BtnSave').show();
            $('#BtnNew').show();
        }
    },
    FillMeasurementFields: function (record) {
        empr_StichingOrder.SetSuitTypeValue(record.suiT_TYPE);
        $("#KURTA_LENGTH").val(record.kurtA_LENGTH ?? '');
        $("#SHOULDER").val(record.shoulder ?? '');
        $("#SLEEVES").val(record.sleeves ?? '');
        $("#CHEST").val(record.chest ?? '');
        $("#WAIST").val(record.waist ?? '');
        $("#COLLAR_SIZE").val(record.collaR_SIZE ?? '');
        $("#ARMHOLE").val(record.armhole ?? '');
        $("#CUFF_MORI").val(record.cufF_MORI ?? '');
        $("#BOTTOM_LENGTH").val(record.bottoM_LENGTH ?? '');
        $("#PANCHA").val(record.pancha ?? '');
        $("#ASAN_GHERA").val(record.asaN_GHERA ?? '');
        $('input[name="BOTTOM_TYPE"]').prop('checked', false);
        $('input[name="DAMAN_STYLE"]').prop('checked', false);
        $('input[name="GALA_STYLE"]').prop('checked', false);
        $('input[name="PATTI_STYLE"]').prop('checked', false);
        $('input[name="FRONT_POCKET"]').prop('checked', false);
        $('input[name="SIDE_POCKETS"]').prop('checked', false);
        $('input[name="FITTING_STYLE"]').prop('checked', false);
        if (record.bottoM_TYPE) {
            $('input[name="BOTTOM_TYPE"][value="' + record.bottoM_TYPE + '"]').prop('checked', true);
        }
        if (record.damaN_STYLE) {
            $('input[name="DAMAN_STYLE"][value="' + record.damaN_STYLE + '"]').prop('checked', true);
        }
        if (record.galA_STYLE) {
            $('input[name="GALA_STYLE"][value="' + record.galA_STYLE + '"]').prop('checked', true);
        }
        if (record.pattI_STYLE) {
            $('input[name="PATTI_STYLE"][value="' + record.pattI_STYLE + '"]').prop('checked', true);
        }
        if (record.fronT_POCKET) {
            $('input[name="FRONT_POCKET"][value="' + record.fronT_POCKET + '"]').prop('checked', true);
        }
        if (record.sidE_POCKETS) {
            $('input[name="SIDE_POCKETS"][value="' + record.sidE_POCKETS + '"]').prop('checked', true);
        }
        if (record.fittinG_STYLE) {
            $('input[name="FITTING_STYLE"][value="' + record.fittinG_STYLE + '"]').prop('checked', true);
        }
    },
    GetCustomerById: function (id) {
        ajaxHelper.ajaxGetJson('/StichingOrder/GetCustomerById?id=' + id, function (data) {
            if (data.msgType == 1) {
                empr_StichingOrder.BindCustomerLookup(data.data);
            } else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },
    GetCustomerByContactNo: function (contactNo) {
        ajaxHelper.ajaxGetJson('/StichingOrder/GetCustomerByContactNo?contactNo=' + encodeURIComponent(contactNo), function (data) {
            if (data.msgType == 1) {
                empr_StichingOrder.BindCustomerLookup(data.data);
            } else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },
    GetOrderById: function (id) {
        ajaxHelper.ajaxGetJson('/StichingOrder/GetOrderById?id=' + id, function (data) {
            empr_StichingOrder.ResetForm();
            if (data.msgType == 1) {
                var record = data.data;
                $("#ORDER_ID").val(record.ordeR_ID || '');
                $("#ID").val(record.id || '');
                $("#FULL_NAME").val(record.fulL_NAME || '');
                $("#CONTACT_NO").val(record.contacT_NO || '');
                empr_StichingOrder.FillMeasurementFields(record);
                $('.modal').modal('hide');
                if (Permissions != "Admin") {
                    if (Permissions.r_DLT) {
                        $('#BtnDelete').show();
                    }
                    if (Permissions.r_ADD) {
                        $('#BtnNew').show();
                    }
                    if (Permissions.r_EDIT) {
                        $('#BtnSave').show();
                    }
                    else {
                        $('#BtnSave').hide();
                    }
                } else {
                    $('#BtnSave').show();
                    $('#BtnDelete').show();
                    $('#BtnNew').show();
                }
            } else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },
    CreateQuickSearchGrid: function (dataSrc) {
        var col = [{
            dataField: "Action",
            width: 100,
            alignment: 'center',
            fixed: true,
            fixedPosition: "left",
            allowExporting: false,
            cellTemplate: function (container, options) {
                var html = '<div class="btn-group btn-group-sm">';
                html += `<a href="javascript:;" class="grid-action-icon elm_edit" style="padding-left: 6px;" reportid=${options.data.ordeR_ID} title="Edit"><i class="fa fa-edit"></i></a>`;
                html += '</div>';
                $(html).appendTo(container);
            }
        },
            { dataField: 'id', caption: 'Serial No.' },
            { dataField: 'fulL_NAME', caption: 'Customer Name' },
            { dataField: 'contacT_NO', caption: 'Contact Number' },
            { dataField: 'suiT_TYPE', caption: 'Suit Type' },
            { dataField: 'adD_USER_ID', caption: 'Created By', visible: false },
            { dataField: 'adD_DATE', caption: 'Created Date', visible: false, dataType: 'date', format: 'dd-MM-yyy' },
            { dataField: 'adD_COMPUTER_NAME', caption: 'Created Computer', visible: false },
            { dataField: 'adD_IP_ADDRESS', caption: 'Created IP', visible: false },
            { dataField: 'adD_POSTALCODE', caption: 'Created PostalCode', visible: false },
            { dataField: 'ediT_USER_ID', caption: 'Updated By', visible: false },
            { dataField: 'ediT_COMPUTER_NAME', caption: 'Updated Computer', visible: false },
            { dataField: 'ediT_DATE', caption: 'Updated Date', visible: false, dataType: 'date', format: 'dd-MM-yyy' },
            { dataField: 'ediT_POSTALCODE', caption: 'Updated PostalCode', visible: false }
        ];
        empr_helper.dxGridbindingVouchers('#gridContainer', col, dataSrc, "StichingOrderQS");
    },
    InitSuitType: function () {
        ati_dxHelper.createDropdownSingle("SUIT_TYPE", SuitTypes, null, "key", "value", "Select", function () { });
    },
    SetSuitTypeValue: function (val) {
        var instance = $('#SUIT_TYPE').dxSelectBox('instance');
        if (!instance) {
            return;
        }
        if (val === undefined || val === null || val === '') {
            instance.option('value', null);
            return;
        }
        var parsed = parseInt(val);
        instance.option('value', isNaN(parsed) ? null : parsed);
    }
};
