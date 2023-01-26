function filterGroup(event) {
    $table.bootstrapTable('filterBy', { group: event.target.value }, {
        'filterAlgorithm': (row, filters) => {
            if (filters.group === "0") {
                return true;
            }
            return row.group === filters.group;
        }
    });
    setTimeout(() => document.getElementById("groupsSelect").value = event.target.value, 10);
}

function filterShift(event) {
    $table.bootstrapTable('filterBy', { shift: event.target.value }, {
        'filterAlgorithm': (row, filters) => {
            if (filters.shift === "0") {
                return true;
            }
            return row.shift === filters.shift;
        }
    });
    setTimeout(() => document.getElementById("shiftsSelect").value = event.target.value, 10);
}

function filterCabinet(event) {
    $table.bootstrapTable('filterBy', { cabinet: event.target.value }, {
        'filterAlgorithm': (row, filters) => {
            if (filters.cabinet === "0") {
                return true;
            }
            return row.cabinet === filters.cabinet;
        }
    });
    setTimeout(() => document.getElementById("cabinetsSelect").value = event.target.value, 10);
}