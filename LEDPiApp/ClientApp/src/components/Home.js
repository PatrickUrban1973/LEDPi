import React, { Component } from 'react';
import Button from 'react-bootstrap/Button';

import DropdownButton from 'react-bootstrap/DropdownButton'
import Dropdown from 'react-bootstrap/Dropdown'
import InputGroup from 'react-bootstrap/InputGroup'
import Form from 'react-bootstrap/Form'
import FormControl from 'react-bootstrap/FormControl'

export class Home extends Component {
    static displayName = Home.name;


    constructor(props) {
    super(props);
        this.state = {scrollingText: [], selectedmodule: [], modules: [], loading: true };
        this.setSelectData = this.setSelectData.bind(this);
        this.handleChange = this.handleChange.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);
    }

    componentDidMount() {
        this.populateModuleData();
    }

    handleChange(event) {
        this.setState({ scrollingText: event.target.value, selectedmodule: this.state.selectedmodule, modules: this.state.modules, loading: false });
    }

    handleSubmit(event) {
        fetch('ledpimodule/process?module=' + this.state.selectedmodule.id + '&scrollingText=' + this.state.scrollingText,
                    {
                        method: "POST",
                        headers: {'Content-Type': 'application/json'}
                    }).
                then(response => console.log(response));                
    }

    setSelectData(e) {

    this.setState({ scrollingText: this.state.scrollingText, selectedmodule: e, modules: this.state.modules, loading: false });
    }


    render() {
        return (
            <Form onSubmit={this.handleSubmit}>

                <DropdownButton id="dropdown-basic-button" title={this.state.selectedmodule.name}>
                    {this.state.modules.map(module =>
                        <Dropdown.Item eventKey={module.id} onSelect={(e) => this.setSelectData(module)}>{module.name}</Dropdown.Item>)}
                </DropdownButton>

                <InputGroup size="sm" className="mb-3">
                        <InputGroup.Prepend>
                        <InputGroup.Text id="inputGroup-sizing-sm">ScrollText</InputGroup.Text>
                        </InputGroup.Prepend>
                    <FormControl aria-label="Small" aria-describedby="inputGroup-sizing-sm" value={this.state.scrollingText} onChange={this.handleChange}/>
                        </InputGroup>
                <Button variant="primary" type="submit">
                    Start
                </Button>
            </Form>

    );
    }

    async populateModuleData() {
        const response = await fetch('ledpimodule');
        const data = await response.json();
        this.setState({ selectedmodule: data[0], modules: data, loading: false });
    }

}
