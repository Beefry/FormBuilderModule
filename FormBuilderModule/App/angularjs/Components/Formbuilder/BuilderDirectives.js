angular.module('formbuilder')
	.directive('formbuilder',function(){
		return {
			templateUrl:'/Templates/Builder.htm',
			restrict:'E',
			scope: {
				FormBuilderID: "@id"
			},
			controllerAs: 'Builder',
			controller: ['$scope','$window','formAPI','redirectPath',function($scope,$window,formAPI,redirectPath){
				var builder = this;
				var Section = function() {
					this.ID = null;
					this.FormID = $scope.model.ID;
					this.SortOrder = null;
					this.Name = "";
					this.Fields = [];
					this.newFieldType = "";
				};
				var Field = function(section) {
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
				};

				var Option = function(field) {
					this.FieldID = field.ID;
					this.Value = "";
				};

				var Form = function () {
					this.ID = null;
					this.Name = null;
					this.Description = null;
					this.Sections = [];
				};

				$scope.model = new Form();
				$scope.newFieldType = "";

				$scope.addField = function(section) {
					try {
						var newField = new Field(section);
						section.Fields.push(newField);
						console.log(newField);
						console.log(section);
					} catch (error) {
						//handle error message
						throw error;
						delete newField;
					}
					section.newFieldType = "";
				};

				$scope.removeField = function(section,field) {
					//Do confirmation here.
					section.Fields = section.Fields.filter(function(element) {
						return element !== field;
					});
				};

				$scope.addNewOption = function(field) {
					//for debug only consolidate later
					var newOption = new Option(field);
					field.Options.push(newOption);
				};

				$scope.removeOption = function(field,option) {
					field.Options = field.Options.filter(function(elem){
						return elem !== option;
					});
				};

				$scope.addSection = function() {
					$scope.model.Sections.push(new Section());
				}

				$scope.removeSection = function(section) {
					$scope.model.Sections = $scope.model.Sections.filter(function(currSection) {
						return currSection != section;
					})
				}

				$scope.saveForm = function() {
					for(var i = 0; i < $scope.model.Sections.length-1; i++) {
						delete $scope.model.Sections[i].newFieldType;
					}
					$scope.model.Sections.map(function(section,sectionIndex) {
						section.SortOrder = sectionIndex;
						section.Fields.map(function(field,fieldIndex) {
							field.SortOrder = fieldIndex;
						})
					});
					console.log($scope.model);
					formAPI.save($scope.model,function(data) {
						console.log(data.result);
						if(data.result == "success") {
							$window.location.href = redirectPath;
						} else if (data.result == "error") {
							//Display Error Message
						} else {
							//Result unknown.
						}
					});
				};

				$scope.hasOptions = function (type) {
					switch(type) {
						case 'textbox':
						case 'textarea':
						case 'text':
							return false;
						case 'select':
						case 'checkbox':
						case 'radio':
							return true;
						case '':
							return false;
							break;
						default:
							return false;
					}
				};

				// if(typeof $scope.FormBuilderID != "undefined") {
					console.log($scope.FormBuilderID);
				// }

				if(typeof $scope.FormBuilderID != "undefined") {
					formAPI.get($scope.FormBuilderID,function(data) {
						console.log(data);
						$scope.model = data;
					});
				}
			}]
		}
	})
	.directive('formdisplay',function(){
		return {
			templateUrl:'/Templates/Displayer.htm',
			restrict:'E',
			scope: {
				FormBuilderID: "@id"
			},
			controllerAs: 'Displayer',
			controller: ['$scope','formAPI',function($scope,formAPI){
				var builder = this;

				console.log($scope.FormBuilderID);

				if(typeof $scope.FormBuilderID != "undefined") {
					formAPI.get($scope.FormBuilderID,function(data) {
						console.log(data);
						$scope.model = data;
					});
				}
			}]
		}
	});;