angular.module('formbuilder')
	.directive('formbuilder',function(){
		return {
			templateUrl:'../Templates/Builder.htm',
			restrict:'E',
			controllerAs: 'Builder',
			controller: ['$scope','formAPI','preLoadedFBObject',function($scope,formAPI,preLoadedFBObject){
				var builder = this;
				var config = {
					Field: function() {
						this.Type = $scope.newFieldType;
						this.FormID = $scope.model.ID;
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
							this.Value = '';
						if($scope.model.Fields.length > 0) {
							this.SortOrder = ($scope.model.Fields.reduce(function(prev,curr){
							    if (curr.SortOrder > prev)
							        return curr.SortOrder;
								else
									return prev;
							},0))+1;
						} else {
						    this.SortOrder = 1;
						}
					},
					Option: function(field) {
						this.FieldID = field.ID;
						if(field.Options.length == 0) {
							this.SortOrder = 0;
						} else {
						    this.SortOrder = (field.Options.reduce(function (prev, curr) {
						        if (curr.SortOrder > prev)
						            return curr.SortOrder;
								else
									return prev;
							},0))+1;
						}
						this.Value = '';
					}
				};

				function Form() {
					this.ID = null;
					this.Name = null;
					this.Description = null;
					this.Fields = [];
				}

				$scope.model = new Form();
				$scope.newFieldType = "";

				$scope.addNewField = function() {
					try {
						var newField = new config.Field();
						$scope.model.Fields.push(newField);
					} catch (error) {
						//handle error message
						throw error;
						delete newField;
					}
					$scope.newFieldType = "";
				};

				$scope.removeField = function(id) {
					//Do confirmation here.
					$scope.model.fields = $scope.model.fields.filter(function(element) {
						return element.id !== id;
					});
				};

				$scope.fieldChanged = function() {
					console.log($scope.model);
				};

				$scope.addNewOption = function(field) {
					//for debug only consolidate later
					var newOption = new config.Option(field);
					field.Options.push(newOption);
				};

				$scope.removeOption = function(field,option) {
					field.Options = field.Options.filter(function(elem){
						return elem !== option;
					});
				};

				$scope.saveForm = function() {
					formAPI.save($scope.model,function(data) {
					});
				};
			}]
		}
	});