import React, { FunctionComponent } from "react";
import { useHistory, useLocation } from 'react-router-dom';

interface LivenessMenuProps {
    pollingInterval: number,
    running: boolean
    onRunningClick: () => void;
}

const LivenessMenu: FunctionComponent<LivenessMenuProps> = ({ running, onRunningClick, pollingInterval }) => {
    const history = useHistory();
    const location = useLocation();
    
    const isGridView = location.pathname === '/healthchecks-grid';

    const toggleView = () => {
        if (isGridView) {
            history.push('/healthchecks');
        } else {
            history.push('/healthchecks-grid');
        }
    };

    return (
        <div className="hc-refesh-group">
            <span>Polling interval: <b>{pollingInterval}</b> secs</span>
            <div className="hc-view-toggle">
                <button
                    onClick={toggleView}
                    type="button"
                    className="hc-view-toggle__button">
                    {isGridView ? (
                        <>
                            <i className="material-icons" style={{ fontSize: '1rem', verticalAlign: 'middle', marginRight: '0.25rem' }}>view_list</i>
                            Table View
                        </>
                    ) : (
                        <>
                            <i className="material-icons" style={{ fontSize: '1rem', verticalAlign: 'middle', marginRight: '0.25rem' }}>grid_view</i>
                            Grid View
                        </>
                    )}
                </button>
                <button
                    onClick={onRunningClick}
                    type="button"
                    className="hc-button">
                    {running ? "Stop polling" : "Start polling"}
                </button>
            </div>
        </div>)
};

export { LivenessMenu };
