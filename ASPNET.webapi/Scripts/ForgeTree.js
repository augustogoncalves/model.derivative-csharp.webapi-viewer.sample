/////////////////////////////////////////////////////////////////////
// Copyright (c) Autodesk, Inc. All rights reserved
// Written by Forge Partner Development
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
/////////////////////////////////////////////////////////////////////

$(document).ready(function () {
    prepareDataManagementTree();
    $('#refreshBuckets').click(function () {
        $('#appBuckets').jstree(true).refresh();
    });
});

function prepareDataManagementTree() {
    $('#appBuckets').jstree({
        'core': {
            'themes': { "icons": true },
            'data': {
                "url": '/api/forge/tree',
                "dataType": "json",
                'multiple': false,
                "data": function (node) {
                    return { "id": node.id };
                }
            }
        },
        'types': {
            'default': {
                'icon': 'glyphicon glyphicon-question-sign'
            },
            '#': {
                'icon': 'glyphicon glyphicon-cloud'
            },
            'bucket': {
                'icon': 'glyphicon glyphicon-folder-open'
            },
            'object': {
                'icon': 'glyphicon glyphicon-file'
            }
        },
        "plugins": ["types", "state", "sort", "contextmenu"]//,
        //contextmenu: { items: autodeskCustomMenu }
    }).on('loaded.jstree', function () {
        $('#appBuckets').jstree('open_all');
    }).bind("activate_node.jstree", function (evt, data) {
        if (data != null && data.node != null) {
            launchViewer(data.node.id);
        }
    });
}