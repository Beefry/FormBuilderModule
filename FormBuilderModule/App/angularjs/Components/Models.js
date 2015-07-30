function Section() {
	this.ID = null;
	this.FormID = $scope.model.ID;
	this.SortOrder = null;
	this.Name = "";
	this.Fields = [];
	this.newFieldType = "";
}
function Field(section) {
	this.Type = section.newFieldType;
	this.SectionID = section.ID;
	this.Values = [];

	switch (this.Type) {
		case 'textbox':
		case 'textarea':
		case 'text':
			this.hasOptions = false;
			break;
		case 'select':
		case 'checkbox':
		case 'radio':
			this.hasOptions = true;
			this.Options = [];
			break;
		case '':
			throw new Error("Please select a field type first.");
			break;
		default:
			throw new Error("Field type not supported. Type given: " + this.Type);
			break;
	}

	if (this.Type == 'text')
		this.Options = [new Option(this)];
	else
		this.Options = [];

	if(section.Fields.length > 0) {
		this.SortOrder = (section.Fields.reduce(function(prev,curr){
		    if (curr.SortOrder > prev)
		        return curr.SortOrder;
			else
				return prev;
		},0))+1;
	} else {
	    this.SortOrder = 1;
	}
}

function Option(field) {
	this.FieldID = field.ID;
	this.Value = "";
}

function Template () {
	this.ID = null;
	this.Name = null;
	this.Description = null;
	this.Sections = [];
}