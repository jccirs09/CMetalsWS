import { useState } from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./ui/card";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import { Input } from "./ui/input";
import { Label } from "./ui/label";
import { Textarea } from "./ui/textarea";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "./ui/select";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "./ui/tabs";
import { Separator } from "./ui/separator";
import {
  Calendar,
  Save,
  Send,
  ArrowLeft,
  AlertCircle,
  CheckCircle,
  Package,
  Settings,
  Clock,
  User,
  Building2,
  FileText
} from "lucide-react";
import { toast } from "sonner@2.0.3";

interface LineItem {
  id: string;
  product: string;
  gauge: string;
  width: string;
  length: string;
  quantity: number;
  weight: number;
  color?: string;
  grade?: string;
  coating?: string;
}

interface WorkOrderForm {
  salesOrder: string;
  customer: string;
  customerPO: string;
  machine: string;
  priority: "low" | "normal" | "high" | "urgent";
  dueDate: string;
  plannedStart: string;
  plannedEnd: string;
  estimatedLbsPerHour: number;
  assignedOperator: string;
  notes: string;
  lineItems: LineItem[];
}

const initialForm: WorkOrderForm = {
  salesOrder: "",
  customer: "",
  customerPO: "",
  machine: "",
  priority: "normal",
  dueDate: "",
  plannedStart: "",
  plannedEnd: "",
  estimatedLbsPerHour: 1000,
  assignedOperator: "",
  notes: "",
  lineItems: []
};

const initialLineItem: LineItem = {
  id: "",
  product: "",
  gauge: "",
  width: "",
  length: "",
  quantity: 1,
  weight: 0,
  color: "",
  grade: "",
  coating: ""
};

const machines = [
  "CTL Line 1",
  "CTL Line 2", 
  "Slitter 1",
  "Slitter 2",
  "Coil Processing",
  "Sheet Cutting",
  "Picking",
  "Packing"
];

const customers = [
  "Industrial Metals Co",
  "Precision Parts LLC",
  "Metro Construction",
  "Steel Solutions Inc",
  "Advanced Manufacturing",
  "BuildTech Industries"
];

const operators = [
  "Mike Johnson",
  "Sarah Chen",
  "David Rodriguez",
  "Lisa Thompson",
  "James Wilson",
  "Maria Garcia"
];

const gauges = ["10 GA", "12 GA", "14 GA", "16 GA", "18 GA", "20 GA", "22 GA", "24 GA"];
const colors = ["Galvanized", "Mill Finish", "White", "Tan", "Brown", "Black", "Blue", "Red"];
const grades = ["A36", "A572-50", "A588", "A709-50", "1008", "1010"];
const coatings = ["None", "Galvanized", "Galvalume", "Painted", "Powder Coated"];

interface WorkOrderCreateProps {
  onNavigate?: (page: string) => void;
}

export function WorkOrderCreate({ onNavigate }: WorkOrderCreateProps = {}) {
  const [form, setForm] = useState<WorkOrderForm>(initialForm);
  const [currentLineItem, setCurrentLineItem] = useState<LineItem>(initialLineItem);
  const [editingIndex, setEditingIndex] = useState<number | null>(null);
  const [activeTab, setActiveTab] = useState("basic");

  const updateForm = (field: keyof WorkOrderForm, value: any) => {
    setForm(prev => ({ ...prev, [field]: value }));
  };

  const updateLineItem = (field: keyof LineItem, value: any) => {
    setCurrentLineItem(prev => ({ ...prev, [field]: value }));
  };

  const calculateWeight = (gauge: string, width: string, length: string, quantity: number) => {
    // Simplified weight calculation for demo
    const gaugeNum = parseInt(gauge.replace(" GA", ""));
    const widthNum = parseFloat(width.replace('"', ""));
    const lengthNum = parseFloat(length.replace('"', ""));
    
    if (gaugeNum && widthNum && lengthNum) {
      // Steel weight calculation: thickness × width × length × density × quantity
      const thickness = 0.135 - (gaugeNum * 0.005); // Approximate thickness in inches
      const weightPerSqFt = thickness * 40.8; // Steel density approximation
      const area = (widthNum * lengthNum) / 144; // Convert to sq ft
      return Math.round(area * weightPerSqFt * quantity);
    }
    return 0;
  };

  const addLineItem = () => {
    if (!currentLineItem.product || !currentLineItem.gauge) {
      toast.error("Please fill in required fields");
      return;
    }

    const weight = calculateWeight(currentLineItem.gauge, currentLineItem.width, currentLineItem.length, currentLineItem.quantity);
    const newItem = {
      ...currentLineItem,
      id: `LI-${Date.now()}`,
      weight
    };

    if (editingIndex !== null) {
      const updatedItems = [...form.lineItems];
      updatedItems[editingIndex] = newItem;
      updateForm("lineItems", updatedItems);
      setEditingIndex(null);
    } else {
      updateForm("lineItems", [...form.lineItems, newItem]);
    }

    setCurrentLineItem(initialLineItem);
    toast.success("Line item added successfully");
  };

  const editLineItem = (index: number) => {
    setCurrentLineItem(form.lineItems[index]);
    setEditingIndex(index);
  };

  const removeLineItem = (index: number) => {
    const updatedItems = form.lineItems.filter((_, i) => i !== index);
    updateForm("lineItems", updatedItems);
    toast.success("Line item removed");
  };

  const validateForm = () => {
    const required = ['salesOrder', 'customer', 'machine', 'dueDate', 'plannedStart', 'plannedEnd'];
    const missing = required.filter(field => !form[field as keyof WorkOrderForm]);
    
    if (missing.length > 0) {
      toast.error(`Please fill in required fields: ${missing.join(', ')}`);
      return false;
    }
    
    if (form.lineItems.length === 0) {
      toast.error("Please add at least one line item");
      return false;
    }
    
    return true;
  };

  const saveDraft = () => {
    toast.success("Work order saved as draft");
  };

  const submitWorkOrder = () => {
    if (!validateForm()) return;
    
    // Generate work order ID
    const woId = `WO-${Date.now()}`;
    console.log("Creating work order:", { ...form, id: woId });
    
    toast.success(`Work order ${woId} created successfully`);
    
    // Reset form
    setForm(initialForm);
    setCurrentLineItem(initialLineItem);
    setActiveTab("basic");
  };

  const totalWeight = form.lineItems.reduce((sum, item) => sum + item.weight, 0);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button 
            variant="ghost" 
            size="sm"
            onClick={() => onNavigate?.("work-orders")}
          >
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Work Orders
          </Button>
          <div>
            <h1>Create Work Order</h1>
            <p className="text-muted-foreground">
              Create a new production work order with line items and scheduling
            </p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" onClick={saveDraft}>
            <Save className="h-4 w-4 mr-2" />
            Save Draft
          </Button>
          <Button className="bg-teal-600 hover:bg-teal-700" onClick={submitWorkOrder}>
            <Send className="h-4 w-4 mr-2" />
            Create Work Order
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Main Form */}
        <div className="lg:col-span-2">
          <Tabs value={activeTab} onValueChange={setActiveTab} className="space-y-6">
            <TabsList className="grid w-full grid-cols-4">
              <TabsTrigger value="basic">Basic Info</TabsTrigger>
              <TabsTrigger value="scheduling">Scheduling</TabsTrigger>
              <TabsTrigger value="line-items">Line Items</TabsTrigger>
              <TabsTrigger value="review">Review</TabsTrigger>
            </TabsList>

            <TabsContent value="basic" className="space-y-6">
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <Building2 className="h-5 w-5" />
                    Order Information
                  </CardTitle>
                  <CardDescription>Basic work order details and customer information</CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <Label htmlFor="salesOrder">Sales Order *</Label>
                      <Input
                        id="salesOrder"
                        placeholder="SO-12345"
                        value={form.salesOrder}
                        onChange={(e) => updateForm("salesOrder", e.target.value)}
                      />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="customerPO">Customer PO</Label>
                      <Input
                        id="customerPO"
                        placeholder="PO-67890"
                        value={form.customerPO}
                        onChange={(e) => updateForm("customerPO", e.target.value)}
                      />
                    </div>
                  </div>

                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <Label htmlFor="customer">Customer *</Label>
                      <Select value={form.customer} onValueChange={(value) => updateForm("customer", value)}>
                        <SelectTrigger>
                          <SelectValue placeholder="Select customer" />
                        </SelectTrigger>
                        <SelectContent>
                          {customers.map((customer) => (
                            <SelectItem key={customer} value={customer}>{customer}</SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="priority">Priority</Label>
                      <Select value={form.priority} onValueChange={(value) => updateForm("priority", value)}>
                        <SelectTrigger>
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="low">Low</SelectItem>
                          <SelectItem value="normal">Normal</SelectItem>
                          <SelectItem value="high">High</SelectItem>
                          <SelectItem value="urgent">Urgent</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="notes">Notes</Label>
                    <Textarea
                      id="notes"
                      placeholder="Additional notes or special instructions..."
                      value={form.notes}
                      onChange={(e) => updateForm("notes", e.target.value)}
                      rows={3}
                    />
                  </div>
                </CardContent>
              </Card>
            </TabsContent>

            <TabsContent value="scheduling" className="space-y-6">
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <Calendar className="h-5 w-5" />
                    Production Scheduling
                  </CardTitle>
                  <CardDescription>Machine assignment and timing details</CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <Label htmlFor="machine">Machine *</Label>
                      <Select value={form.machine} onValueChange={(value) => updateForm("machine", value)}>
                        <SelectTrigger>
                          <SelectValue placeholder="Select machine" />
                        </SelectTrigger>
                        <SelectContent>
                          {machines.map((machine) => (
                            <SelectItem key={machine} value={machine}>{machine}</SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="operator">Assigned Operator</Label>
                      <Select value={form.assignedOperator} onValueChange={(value) => updateForm("assignedOperator", value)}>
                        <SelectTrigger>
                          <SelectValue placeholder="Select operator" />
                        </SelectTrigger>
                        <SelectContent>
                          {operators.map((operator) => (
                            <SelectItem key={operator} value={operator}>{operator}</SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>
                  </div>

                  <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                    <div className="space-y-2">
                      <Label htmlFor="plannedStart">Planned Start *</Label>
                      <Input
                        id="plannedStart"
                        type="datetime-local"
                        value={form.plannedStart}
                        onChange={(e) => updateForm("plannedStart", e.target.value)}
                      />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="plannedEnd">Planned End *</Label>
                      <Input
                        id="plannedEnd"
                        type="datetime-local"
                        value={form.plannedEnd}
                        onChange={(e) => updateForm("plannedEnd", e.target.value)}
                      />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="dueDate">Due Date *</Label>
                      <Input
                        id="dueDate"
                        type="date"
                        value={form.dueDate}
                        onChange={(e) => updateForm("dueDate", e.target.value)}
                      />
                    </div>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="estimatedLbsPerHour">Estimated Lbs/Hour</Label>
                    <Input
                      id="estimatedLbsPerHour"
                      type="number"
                      value={form.estimatedLbsPerHour}
                      onChange={(e) => updateForm("estimatedLbsPerHour", parseInt(e.target.value) || 0)}
                    />
                  </div>
                </CardContent>
              </Card>
            </TabsContent>

            <TabsContent value="line-items" className="space-y-6">
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <Package className="h-5 w-5" />
                    Add Line Item
                  </CardTitle>
                  <CardDescription>Add products to this work order</CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                    <div className="space-y-2">
                      <Label htmlFor="product">Product Description *</Label>
                      <Input
                        id="product"
                        placeholder="Hot Rolled Coil - 16 GA x 48..."
                        value={currentLineItem.product}
                        onChange={(e) => updateLineItem("product", e.target.value)}
                      />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="gauge">Gauge *</Label>
                      <Select value={currentLineItem.gauge} onValueChange={(value) => updateLineItem("gauge", value)}>
                        <SelectTrigger>
                          <SelectValue placeholder="Select gauge" />
                        </SelectTrigger>
                        <SelectContent>
                          {gauges.map((gauge) => (
                            <SelectItem key={gauge} value={gauge}>{gauge}</SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="quantity">Quantity</Label>
                      <Input
                        id="quantity"
                        type="number"
                        value={currentLineItem.quantity}
                        onChange={(e) => updateLineItem("quantity", parseInt(e.target.value) || 1)}
                      />
                    </div>
                  </div>

                  <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                    <div className="space-y-2">
                      <Label htmlFor="width">Width</Label>
                      <Input
                        id="width"
                        placeholder='48"'
                        value={currentLineItem.width}
                        onChange={(e) => updateLineItem("width", e.target.value)}
                      />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="length">Length</Label>
                      <Input
                        id="length"
                        placeholder='120"'
                        value={currentLineItem.length}
                        onChange={(e) => updateLineItem("length", e.target.value)}
                      />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="color">Color/Finish</Label>
                      <Select value={currentLineItem.color} onValueChange={(value) => updateLineItem("color", value)}>
                        <SelectTrigger>
                          <SelectValue placeholder="Select color" />
                        </SelectTrigger>
                        <SelectContent>
                          {colors.map((color) => (
                            <SelectItem key={color} value={color}>{color}</SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="grade">Grade</Label>
                      <Select value={currentLineItem.grade} onValueChange={(value) => updateLineItem("grade", value)}>
                        <SelectTrigger>
                          <SelectValue placeholder="Select grade" />
                        </SelectTrigger>
                        <SelectContent>
                          {grades.map((grade) => (
                            <SelectItem key={grade} value={grade}>{grade}</SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>
                  </div>

                  <div className="flex items-center justify-between">
                    <div className="text-sm text-muted-foreground">
                      Est. Weight: {calculateWeight(currentLineItem.gauge, currentLineItem.width, currentLineItem.length, currentLineItem.quantity).toLocaleString()} lbs
                    </div>
                    <Button onClick={addLineItem} className="bg-teal-600 hover:bg-teal-700">
                      {editingIndex !== null ? "Update Item" : "Add Item"}
                    </Button>
                  </div>
                </CardContent>
              </Card>

              {form.lineItems.length > 0 && (
                <Card>
                  <CardHeader>
                    <CardTitle>Line Items ({form.lineItems.length})</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <div className="space-y-3">
                      {form.lineItems.map((item, index) => (
                        <div key={item.id} className="flex items-center justify-between p-3 border rounded-lg">
                          <div className="space-y-1">
                            <p className="font-medium">{item.product}</p>
                            <div className="flex items-center gap-4 text-sm text-muted-foreground">
                              <span>{item.gauge}</span>
                              <span>{item.width} x {item.length}</span>
                              <span>Qty: {item.quantity}</span>
                              <span>{item.weight.toLocaleString()} lbs</span>
                            </div>
                          </div>
                          <div className="flex items-center gap-2">
                            <Button variant="ghost" size="sm" onClick={() => editLineItem(index)}>
                              Edit
                            </Button>
                            <Button variant="ghost" size="sm" onClick={() => removeLineItem(index)}>
                              Remove
                            </Button>
                          </div>
                        </div>
                      ))}
                    </div>
                  </CardContent>
                </Card>
              )}
            </TabsContent>

            <TabsContent value="review" className="space-y-6">
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <CheckCircle className="h-5 w-5" />
                    Review Work Order
                  </CardTitle>
                  <CardDescription>Review all details before creating the work order</CardDescription>
                </CardHeader>
                <CardContent className="space-y-6">
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div className="space-y-4">
                      <h3>Order Information</h3>
                      <div className="space-y-2 text-sm">
                        <div className="flex justify-between">
                          <span className="text-muted-foreground">Sales Order:</span>
                          <span>{form.salesOrder || "Not specified"}</span>
                        </div>
                        <div className="flex justify-between">
                          <span className="text-muted-foreground">Customer:</span>
                          <span>{form.customer || "Not specified"}</span>
                        </div>
                        <div className="flex justify-between">
                          <span className="text-muted-foreground">Customer PO:</span>
                          <span>{form.customerPO || "Not specified"}</span>
                        </div>
                        <div className="flex justify-between">
                          <span className="text-muted-foreground">Priority:</span>
                          <Badge className={
                            form.priority === "urgent" ? "bg-red-100 text-red-700" :
                            form.priority === "high" ? "bg-orange-100 text-orange-700" :
                            form.priority === "normal" ? "bg-blue-100 text-blue-700" :
                            "bg-gray-100 text-gray-700"
                          }>
                            {form.priority}
                          </Badge>
                        </div>
                      </div>
                    </div>

                    <div className="space-y-4">
                      <h3>Scheduling</h3>
                      <div className="space-y-2 text-sm">
                        <div className="flex justify-between">
                          <span className="text-muted-foreground">Machine:</span>
                          <span>{form.machine || "Not specified"}</span>
                        </div>
                        <div className="flex justify-between">
                          <span className="text-muted-foreground">Operator:</span>
                          <span>{form.assignedOperator || "Not assigned"}</span>
                        </div>
                        <div className="flex justify-between">
                          <span className="text-muted-foreground">Planned Start:</span>
                          <span>{form.plannedStart ? new Date(form.plannedStart).toLocaleString() : "Not specified"}</span>
                        </div>
                        <div className="flex justify-between">
                          <span className="text-muted-foreground">Due Date:</span>
                          <span>{form.dueDate || "Not specified"}</span>
                        </div>
                      </div>
                    </div>
                  </div>

                  <Separator />

                  <div className="space-y-4">
                    <div className="flex items-center justify-between">
                      <h3>Line Items Summary</h3>
                      <div className="text-sm text-muted-foreground">
                        Total Weight: {totalWeight.toLocaleString()} lbs
                      </div>
                    </div>
                    {form.lineItems.length > 0 ? (
                      <div className="space-y-2">
                        {form.lineItems.map((item, index) => (
                          <div key={item.id} className="flex items-center justify-between p-2 bg-gray-50 rounded">
                            <div>
                              <p className="font-medium text-sm">{item.product}</p>
                              <p className="text-xs text-muted-foreground">
                                {item.gauge} • {item.width} x {item.length} • Qty: {item.quantity}
                              </p>
                            </div>
                            <div className="text-sm">{item.weight.toLocaleString()} lbs</div>
                          </div>
                        ))}
                      </div>
                    ) : (
                      <div className="flex items-center gap-2 p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
                        <AlertCircle className="h-4 w-4 text-yellow-600" />
                        <span className="text-sm text-yellow-700">No line items added</span>
                      </div>
                    )}
                  </div>

                  {form.notes && (
                    <>
                      <Separator />
                      <div className="space-y-2">
                        <h3>Notes</h3>
                        <p className="text-sm text-muted-foreground">{form.notes}</p>
                      </div>
                    </>
                  )}
                </CardContent>
              </Card>
            </TabsContent>
          </Tabs>
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <FileText className="h-5 w-5" />
                Work Order Summary
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Status:</span>
                  <Badge variant="outline">Draft</Badge>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Line Items:</span>
                  <span>{form.lineItems.length}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Total Weight:</span>
                  <span>{totalWeight.toLocaleString()} lbs</span>
                </div>
                {form.plannedStart && form.plannedEnd && (
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">Duration:</span>
                    <span>
                      {Math.round((new Date(form.plannedEnd).getTime() - new Date(form.plannedStart).getTime()) / (1000 * 60 * 60))}h
                    </span>
                  </div>
                )}
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Clock className="h-5 w-5" />
                Quick Actions
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-2">
              <Button variant="outline" size="sm" className="w-full justify-start">
                Copy from SO
              </Button>
              <Button variant="outline" size="sm" className="w-full justify-start">
                Load Template
              </Button>
              <Button variant="outline" size="sm" className="w-full justify-start">
                Check Inventory
              </Button>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}